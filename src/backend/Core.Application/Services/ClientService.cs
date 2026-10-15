using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Enums;
using FluentValidation;

namespace Core.Application.Services;

public sealed class ClientService(
    IClientRepository clients,
    IDocumentNumberGenerator numbers,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    IValidator<CreateClientRequest> createValidator,
    IValidator<UpdateClientRequest> updateValidator,
    IValidator<ClientPaymentRequest> paymentValidator,
    IValidator<ClientEmptiesReturnRequest> emptiesValidator,
    IValidator<ClientBalanceAdjustmentRequest> adjustmentValidator) : IClientService
{
    public async Task<IReadOnlyList<ClientResponse>> SearchAsync(string? search, CancellationToken cancellationToken = default)
    {
        var list = await clients.SearchAsync(search, receivableOnly: false, cancellationToken);
        return await WithLastPurchaseAsync(list, cancellationToken);
    }

    /// <summary>Cuentas por cobrar: clientes con cualquier saldo negativo (dinero o vacíos).</summary>
    public async Task<IReadOnlyList<ClientResponse>> GetReceivablesAsync(CancellationToken cancellationToken = default)
    {
        var list = await clients.SearchAsync(null, receivableOnly: true, cancellationToken);
        return await WithLastPurchaseAsync(list, cancellationToken);
    }

    public async Task<ClientResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await clients.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el cliente {id}.");
        return (await WithLastPurchaseAsync([client], cancellationToken))[0];
    }

    /// <summary>Estado de cuenta separado en pestañas: facturas, regalías, abonos, vacíos y ajustes.</summary>
    public async Task<ClientStatementResponse> GetStatementAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await GetByIdAsync(id, cancellationToken);
        var movements = (await clients.GetMovementsAsync(id, cancellationToken)).Select(m => m.ToResponse()).ToList();

        List<ClientMovementResponse> Of(params ClientMovementType[] types) => movements.Where(m => types.Contains(m.Type)).ToList();

        return new ClientStatementResponse(
            client,
            Of(ClientMovementType.Invoice),
            Of(ClientMovementType.Gift),
            Of(ClientMovementType.Payment),
            Of(ClientMovementType.EmptiesGenerated, ClientMovementType.EmptiesReturned, ClientMovementType.InvoiceCancelled),
            Of(ClientMovementType.BalanceAdjustment));
    }

    /// <summary>Alta: código correlativo atómico, tipo "Ocasional" y saldos en 0.</summary>
    public async Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var client = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            string rif = request.Rif.Trim().ToUpperInvariant();
            if (await clients.RifExistsAsync(rif, ct))
            {
                throw new InvalidOperationException($"Ya existe un cliente con el RIF o cédula {rif}.");
            }

            int code = await numbers.NextAsync(DocumentSequences.Client, 0, 0, await clients.GetMaxCodeAsync(ct), ct);
            var created = new Client(code, rif, request.Name, request.Address, request.Nickname, Client.DefaultType);
            clients.Add(created);
            await unitOfWork.SaveChangesAsync(ct);
            return created;
        }, cancellationToken);

        await audit.AuditAsync("client_created", "clients", $"Cliente creado: {client.FormattedCode} {client.Name}",
            nameof(Client), client.Id.ToString(), after: client.ToResponse(), cancellationToken: cancellationToken);
        return client.ToResponse();
    }

    /// <summary>Edita datos de contacto. Código, nombre y RIF son inmutables.</summary>
    public async Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var client = await clients.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el cliente {id}.");
        var before = client.ToResponse();

        client.UpdateContact(request.Address, request.Nickname, request.Type);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("client_updated", "clients", $"Cliente actualizado: {client.FormattedCode} {client.Name}",
            nameof(Client), client.Id.ToString(), before: before, after: client.ToResponse(), cancellationToken: cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Borrado lógico (solo Admin). No se permite con saldos pendientes.</summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await clients.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el cliente {id}.");
        if (client.MoneyBalance != 0 || client.EmptyBoxesBalance != 0 || client.EmptyUnitsBalance != 0)
        {
            throw new InvalidOperationException(
                $"No se puede eliminar al cliente {client.Name} porque tiene saldos pendientes de dinero o vacíos.");
        }

        client.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("client_deleted", "clients", $"Cliente eliminado: {client.FormattedCode} {client.Name}",
            nameof(Client), client.Id.ToString(), cancellationToken: cancellationToken);
    }

    /// <summary>Abono de dinero: suma al saldo (negativo = debe) y registra el movimiento.</summary>
    public async Task<ClientResponse> RegisterPaymentAsync(Guid id, ClientPaymentRequest request, CancellationToken cancellationToken = default)
    {
        await paymentValidator.ValidateAndThrowAsync(request, cancellationToken);

        var (before, after) = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var client = await clients.GetForUpdateAsync(id, ct)
                ?? throw new KeyNotFoundException($"No existe el cliente {id}.");
            decimal previous = client.MoneyBalance;
            client.ApplyPayment(request.Amount);

            string observation = string.IsNullOrWhiteSpace(request.Observation) ? string.Empty : $" Observación: {request.Observation.Trim()}";
            clients.AddMovement(new ClientMovement(client.Id, ClientMovementType.Payment, "Abono",
                $"Abono sumado al saldo del cliente.{observation} | Antes: {BusinessMappings.Money(previous)} | Saldo actual: {BusinessMappings.Money(client.MoneyBalance)}",
                request.Amount, null));
            await unitOfWork.SaveChangesAsync(ct);
            return (previous, client.MoneyBalance);
        }, cancellationToken);

        await audit.AuditAsync("client_payment", "clients", $"Abono de {BusinessMappings.Money(request.Amount)}",
            nameof(Client), id.ToString(), request.Amount, new { MoneyBalance = before }, new { MoneyBalance = after },
            cancellationToken: cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Devolución de vacíos fuera de factura, por grupo: suma al saldo de vacíos.</summary>
    public async Task<ClientResponse> ReturnEmptiesAsync(Guid id, ClientEmptiesReturnRequest request, CancellationToken cancellationToken = default)
    {
        await emptiesValidator.ValidateAndThrowAsync(request, cancellationToken);
        var groups = request.Groups.Where(g => g.Boxes > 0 || g.Units > 0).ToList();

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var client = await clients.GetForUpdateAsync(id, ct)
                ?? throw new KeyNotFoundException($"No existe el cliente {id}.");
            var (boxesBefore, unitsBefore) = (client.EmptyBoxesBalance, client.EmptyUnitsBalance);

            foreach (var group in groups)
            {
                client.ApplyEmptiesMovement(group.GroupKey, group.Boxes, group.Units);
            }

            string detail = string.Join(" | ", groups.Select(g =>
                $"{EmptyReturnGroups.GetName(g.GroupKey)}: Devueltos {SpanishText.Quantity(g.Boxes, g.Units)}"));
            string observation = string.IsNullOrWhiteSpace(request.Observation) ? string.Empty : $" | Observación: {request.Observation.Trim()}";
            clients.AddMovement(new ClientMovement(client.Id, ClientMovementType.EmptiesReturned, "Devolución de vacíos",
                $"Antes: {SpanishText.Quantity(boxesBefore, unitsBefore)} | Devueltos: {SpanishText.Quantity(groups.Sum(g => g.Boxes), groups.Sum(g => g.Units))} | " +
                $"Saldo actual: {SpanishText.Quantity(client.EmptyBoxesBalance, client.EmptyUnitsBalance)} | Detalle: {detail}{observation}",
                0m, null));
            await unitOfWork.SaveChangesAsync(ct);
            return true;
        }, cancellationToken);

        await audit.AuditAsync("client_empties_returned", "clients", "Devolución de vacíos fuera de factura",
            nameof(Client), id.ToString(), metadata: groups, cancellationToken: cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Ajuste manual (solo Admin): fija saldos absolutos con signo; motivo obligatorio.</summary>
    public async Task<ClientResponse> AdjustBalancesAsync(Guid id, ClientBalanceAdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        await adjustmentValidator.ValidateAndThrowAsync(request, cancellationToken);

        var before = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var client = await clients.GetForUpdateAsync(id, ct)
                ?? throw new KeyNotFoundException($"No existe el cliente {id}.");
            var previous = new { client.MoneyBalance, client.EmptyBoxesBalance, client.EmptyUnitsBalance };

            client.AdjustBalances(request.MoneyBalance, request.EmptyBoxesBalance, request.EmptyUnitsBalance);
            clients.AddMovement(new ClientMovement(client.Id, ClientMovementType.BalanceAdjustment, "Ajuste manual de saldo",
                $"Antes: {BusinessMappings.Money(previous.MoneyBalance)} y {SpanishText.Quantity(previous.EmptyBoxesBalance, previous.EmptyUnitsBalance)} de vacíos | " +
                $"Después: {BusinessMappings.Money(client.MoneyBalance)} y {SpanishText.Quantity(client.EmptyBoxesBalance, client.EmptyUnitsBalance)} de vacíos | " +
                $"Motivo: {request.Reason.Trim()}",
                client.MoneyBalance - previous.MoneyBalance, null));
            await unitOfWork.SaveChangesAsync(ct);
            return previous;
        }, cancellationToken);

        await audit.AuditAsync("client_balance_adjusted", "clients", $"Ajuste manual de saldo. Motivo: {request.Reason}",
            nameof(Client), id.ToString(), before: before,
            after: new { request.MoneyBalance, request.EmptyBoxesBalance, request.EmptyUnitsBalance },
            metadata: new { request.Reason }, cancellationToken: cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task<IReadOnlyList<ClientResponse>> WithLastPurchaseAsync(IReadOnlyList<Client> list, CancellationToken cancellationToken)
    {
        var lastSales = await clients.GetLastSaleDatesAsync(list.Select(c => c.Id), cancellationToken);
        return list.Select(c => c.ToResponse(lastSales.TryGetValue(c.Id, out var date) ? date : null)).ToList();
    }
}
