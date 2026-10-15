using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.ValueObjects;
using FluentValidation;

namespace Core.Application.Services;

public sealed class InvoiceService(
    IInvoiceRepository invoices,
    IClientRepository clients,
    IProductRepository products,
    ITruckRepository trucks,
    IEmployeeRepository employees,
    IUserRepository users,
    IDocumentNumberGenerator numbers,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    ICurrentUser currentUser,
    IClock clock,
    IDocumentPdfGenerator pdf,
    IValidator<CreateSaleRequest> saleValidator,
    IValidator<CreateGiftRequest> giftValidator,
    IValidator<CancelInvoiceRequest> cancelValidator) : IInvoiceService
{
    /// <summary>Vendedores que el usuario puede elegir; si no puede elegir, usa su vendedor fijo.</summary>
    public async Task<AttendantOptionsResponse> GetAttendantOptionsAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var options = user.CanChangeAttendant
            ? (await employees.GetAllAsync(includeInactive: false, cancellationToken)).Select(e => e.ToResponse()).ToList()
            : [];
        return new AttendantOptionsResponse(user.CanChangeAttendant, user.DefaultAttendant, options);
    }

    /// <summary>
    /// Factura de venta en una sola transacción: valida stock de TODAS las líneas, descuenta inventario del
    /// origen, numera, carga el pendiente al cliente, registra abono y vacíos. Si algo falla, no cambia nada.
    /// </summary>
    public async Task<InvoiceResponse> CreateSaleAsync(CreateSaleRequest request, CancellationToken cancellationToken = default)
    {
        await saleValidator.ValidateAndThrowAsync(request, cancellationToken);
        var (employeeId, attendant) = await ResolveAttendantAsync(request.EmployeeId, cancellationToken);

        var invoice = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var client = await clients.GetForUpdateAsync(request.ClientId, ct)
                ?? throw new InvalidOperationException($"El cliente {request.ClientId} no existe.");
            var truck = await StockOperations.LoadOriginTruckAsync(trucks, request.DispatchOrigin, request.TruckId, ct);
            var lines = await StockOperations.LoadLinesAsync(products, request.Items, ct);

            StockOperations.EnsureAvailable(request.DispatchOrigin, truck, lines);
            foreach (var line in lines)
            {
                StockOperations.Remove(request.DispatchOrigin, truck, line.Product, line.Quantity);
            }

            var returned = (request.ReturnedEmpties ?? [])
                .Where(q => q.Boxes > 0 || q.Units > 0)
                .ToDictionary(q => q.GroupKey, q => new BoxQuantity(q.Boxes, q.Units));

            var created = Invoice.CreateSale(
                await NextNumberAsync(InvoiceType.Sale, ct), client, request.DispatchOrigin, truck, employeeId, attendant,
                lines, request.Payment, request.PaymentObservation, returned, clock.UtcNow);

            invoices.Add(created);
            ApplyAccountEffects(client, created, request.PaymentObservation);
            await unitOfWork.SaveChangesAsync(ct);
            return created;
        }, cancellationToken);

        await audit.AuditAsync("invoice_created", "invoices", $"{invoice.Number} a {invoice.ClientName}",
            nameof(Invoice), invoice.Id.ToString(), invoice.Total,
            metadata: new { invoice.Payment, invoice.Pending, invoice.DispatchOrigin, invoice.TruckName, Items = invoice.Items.Count },
            cancellationToken: cancellationToken);
        return invoice.ToResponse();
    }

    /// <summary>
    /// Regalía: solo productos autorizados, total 0, no afecta el saldo de dinero, sí genera vacíos.
    /// El vendedor es el del usuario conectado.
    /// </summary>
    public async Task<InvoiceResponse> CreateGiftAsync(CreateGiftRequest request, CancellationToken cancellationToken = default)
    {
        await giftValidator.ValidateAndThrowAsync(request, cancellationToken);
        var user = await GetCurrentUserAsync(cancellationToken);

        var invoice = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var client = await clients.GetForUpdateAsync(request.ClientId, ct)
                ?? throw new InvalidOperationException($"El cliente {request.ClientId} no existe.");
            var truck = await StockOperations.LoadOriginTruckAsync(trucks, request.DispatchOrigin, request.TruckId, ct);
            var lines = await StockOperations.LoadLinesAsync(products, request.Items, ct);

            StockOperations.EnsureAvailable(request.DispatchOrigin, truck, lines);
            var created = Invoice.CreateGift(
                await NextNumberAsync(InvoiceType.Gift, ct), client, request.DispatchOrigin, truck, user.DefaultAttendant, lines, clock.UtcNow);
            foreach (var line in lines)
            {
                StockOperations.Remove(request.DispatchOrigin, truck, line.Product, line.Quantity);
            }

            invoices.Add(created);
            ApplyAccountEffects(client, created, null);
            await unitOfWork.SaveChangesAsync(ct);
            return created;
        }, cancellationToken);

        await audit.AuditAsync("gift_invoice_created", "invoices", $"{invoice.Number} a {invoice.ClientName}",
            nameof(Invoice), invoice.Id.ToString(), 0m,
            metadata: new { invoice.GeneratedEmptyBoxes, invoice.GeneratedEmptyUnits }, cancellationToken: cancellationToken);
        return invoice.ToResponse();
    }

    /// <summary>
    /// Anulación (solo Admin): devuelve el inventario al origen, revierte el pendiente de dinero y los vacíos,
    /// marca el documento como anulado y deja el movimiento en el estado de cuenta. Todo o nada.
    /// </summary>
    public async Task<InvoiceResponse> CancelAsync(Guid id, CancelInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        await cancelValidator.ValidateAndThrowAsync(request, cancellationToken);

        var invoice = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var target = await invoices.GetForUpdateAsync(id, ct)
                ?? throw new KeyNotFoundException($"No existe la factura {id}.");
            target.Cancel(request.Reason, currentUser.Username, clock.UtcNow);

            var client = await clients.GetForUpdateAsync(target.ClientId, ct)
                ?? throw new InvalidOperationException("El cliente de la factura ya no existe; no se puede anular.");
            Truck? truck = null;
            if (target.DispatchOrigin == StockOrigin.Truck)
            {
                truck = await trucks.GetForUpdateAsync(target.TruckId!.Value, ct)
                    ?? throw new InvalidOperationException($"El camión {target.TruckName} ya no existe; no se puede devolver el inventario.");
            }

            var found = await products.GetManyForUpdateAsync(target.Items.Select(i => i.ProductId).Distinct(), ct);
            foreach (var item in target.Items)
            {
                var product = found[item.ProductId];
                int before = truck?.GetUnits(product.Id) ?? 0;
                StockOperations.Add(target.DispatchOrigin, truck, product, item.TotalUnits);
                if (truck is not null)
                {
                    trucks.AddLoad(new TruckLoad(truck, product, item.TotalUnits, before, truck.GetUnits(product.Id),
                        TruckLoadSource.InvoiceCancellation, null, $"Reverso por anulación de {target.Number}", currentUser.Username));
                }
            }

            client.RevertInvoicePending(target.Pending);
            foreach (var group in target.EmptyGroups)
            {
                client.ApplyEmptiesMovement(group.GroupKey, group.PendingBoxes, group.PendingUnits);
            }

            string detail = string.Join(" | ", target.EmptyGroups
                .Where(g => g.PendingBoxes != 0 || g.PendingUnits != 0)
                .Select(g => $"{EmptyReturnGroups.GetName(g.GroupKey)}: Devueltos {SpanishText.Quantity(g.PendingBoxes, g.PendingUnits)}"));
            clients.AddMovement(new ClientMovement(client.Id, ClientMovementType.InvoiceCancelled,
                target.Type == InvoiceType.Sale ? "Factura anulada" : "Regalía anulada",
                $"{target.Number} | Motivo: {request.Reason.Trim()} | Dinero revertido: {BusinessMappings.Money(target.Pending)} | " +
                $"Vacíos revertidos: {SpanishText.Quantity(target.PendingEmptyBoxes, target.PendingEmptyUnits)} | " +
                $"Saldo actual: {BusinessMappings.Money(client.MoneyBalance)} y {SpanishText.Quantity(client.EmptyBoxesBalance, client.EmptyUnitsBalance)} de vacíos" +
                (detail.Length > 0 ? $" | Detalle: {detail}" : string.Empty),
                target.Pending, target.Id));

            await unitOfWork.SaveChangesAsync(ct);
            return target;
        }, cancellationToken);

        await audit.AuditAsync("invoice_cancelled", "invoices", $"{invoice.Number} anulada. Motivo: {request.Reason}",
            nameof(Invoice), invoice.Id.ToString(), invoice.Pending,
            metadata: new { request.Reason, invoice.PendingEmptyBoxes, invoice.PendingEmptyUnits }, cancellationToken: cancellationToken);
        return invoice.ToResponse();
    }

    public async Task<InvoiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await invoices.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la factura {id}.");
        return invoice.ToResponse();
    }

    public Task<PagedResponse<InvoiceSummaryResponse>> ListAsync(
        DateOnly? from, DateOnly? to, Guid? clientId, InvoiceType? type, bool includeCancelled, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        DateTime? fromUtc = from is null ? null : clock.GetDayRangeUtc(from.Value).StartUtc;
        DateTime? toUtc = to is null ? null : clock.GetDayRangeUtc(to.Value).EndUtc;
        return invoices.ListAsync(fromUtc, toUtc, clientId, type, includeCancelled,
            Math.Max(1, page), Math.Clamp(pageSize, 1, 200), cancellationToken);
    }

    public async Task<byte[]> GetTicketPdfAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await invoices.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la factura {id}.");
        var client = await clients.GetByIdAsync(invoice.ClientId, cancellationToken);
        return pdf.RenderInvoiceTicket(invoice, client);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetGiftEligibleProductsAsync(CancellationToken cancellationToken = default)
    {
        var list = await products.GetGiftEligibleAsync(cancellationToken);
        return list.Select(p => p.ToResponse()).ToList();
    }

    /// <summary>Efectos en la cuenta del cliente y movimientos del estado de cuenta.</summary>
    private void ApplyAccountEffects(Client client, Invoice invoice, string? paymentObservation)
    {
        bool isGift = invoice.Type == InvoiceType.Gift;
        if (!isGift)
        {
            client.ApplyInvoicePending(invoice.Pending);
        }

        clients.AddMovement(new ClientMovement(client.Id,
            isGift ? ClientMovementType.Gift : ClientMovementType.Invoice,
            invoice.Number,
            isGift
                ? $"Regalía | Total: {BusinessMappings.Money(0)} | Vacíos generados: {SpanishText.Quantity(invoice.GeneratedEmptyBoxes, invoice.GeneratedEmptyUnits)}"
                : $"Total: {BusinessMappings.Money(invoice.Total)} | Abono: {BusinessMappings.Money(invoice.Payment)} | Pendiente: {BusinessMappings.Money(invoice.Pending)}",
            invoice.Total,
            invoice.Id));

        if (!isGift && invoice.Payment > 0)
        {
            string observation = string.IsNullOrWhiteSpace(paymentObservation) ? string.Empty : $". Observación: {paymentObservation.Trim()}";
            clients.AddMovement(new ClientMovement(client.Id, ClientMovementType.Payment, "Abono",
                $"Abono realizado al momento de {invoice.Number}{observation}", invoice.Payment, invoice.Id));
        }

        if (!invoice.HasEmptiesMovement)
        {
            return;
        }

        foreach (var group in invoice.EmptyGroups)
        {
            client.ApplyEmptiesMovement(group.GroupKey, -group.PendingBoxes, -group.PendingUnits);
        }

        string detail = string.Join(" | ", invoice.EmptyGroups.Select(g =>
            $"{EmptyReturnGroups.GetName(g.GroupKey)}: Generados {SpanishText.Quantity(g.GeneratedBoxes, g.GeneratedUnits)}, " +
            $"Devueltos {SpanishText.Quantity(g.ReturnedBoxes, g.ReturnedUnits)}, Pendientes {SpanishText.Quantity(g.PendingBoxes, g.PendingUnits)}"));
        clients.AddMovement(new ClientMovement(client.Id, ClientMovementType.EmptiesGenerated, "Vacíos",
            $"Operación: {invoice.Number} | Generados: {SpanishText.Quantity(invoice.GeneratedEmptyBoxes, invoice.GeneratedEmptyUnits)} | " +
            $"Devueltos: {SpanishText.Quantity(invoice.ReturnedEmptyBoxes, invoice.ReturnedEmptyUnits)} | " +
            $"Movimiento de esta operación: {SpanishText.Quantity(-invoice.PendingEmptyBoxes, -invoice.PendingEmptyUnits)} | " +
            $"Saldo actual: {SpanishText.Quantity(client.EmptyBoxesBalance, client.EmptyUnitsBalance)} | Detalle: {detail}",
            0m, invoice.Id));
    }

    private async Task<string> NextNumberAsync(InvoiceType type, CancellationToken cancellationToken)
    {
        var today = clock.Today;
        string sequence = type == InvoiceType.Sale ? DocumentSequences.Invoice : DocumentSequences.Gift;
        int next = await numbers.NextAsync(sequence, today.Year, today.Month, 0, cancellationToken);
        return SpanishText.InvoiceNumber(type, today.Year, today.Month, next);
    }

    /// <summary>
    /// "Quien atiende": si el usuario puede cambiar de vendedor elige un empleado activo (o usa el suyo por defecto);
    /// si no puede, siempre se usa su vendedor fijo.
    /// </summary>
    private async Task<(Guid? EmployeeId, string Attendant)> ResolveAttendantAsync(Guid? employeeId, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        if (employeeId is null)
        {
            return (null, user.DefaultAttendant);
        }

        if (!user.CanChangeAttendant)
        {
            throw new InvalidOperationException("Su usuario no tiene permiso para cambiar el vendedor que atiende.");
        }

        var employee = await employees.GetByIdAsync(employeeId.Value, cancellationToken)
            ?? throw new InvalidOperationException($"El empleado {employeeId} no existe.");
        if (!employee.IsActive)
        {
            throw new InvalidOperationException($"El empleado {employee.Name} está inactivo.");
        }

        return (employee.Id, employee.Name);
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException("Se requiere un usuario autenticado.");
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("El usuario del token ya no existe.");
        return user.IsActive ? user : throw new UnauthorizedAccessException("Este usuario está inactivo.");
    }
}
