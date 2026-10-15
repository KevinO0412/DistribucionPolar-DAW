using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.ValueObjects;
using FluentValidation;

namespace Core.Application.Services;

public sealed class ConsignmentService(
    IConsignmentRepository consignments,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    IClock clock,
    IDocumentPdfGenerator pdf,
    IValidator<CreateConsignmentRequest> createValidator,
    IValidator<ConsignmentReturnRequest> returnValidator) : IConsignmentService
{
    /// <summary>Crea el evento y descuenta del galpón lo entregado, validando stock en la misma transacción.</summary>
    public async Task<ConsignmentResponse> CreateAsync(CreateConsignmentRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var created = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var lines = await StockOperations.LoadLinesAsync(products, request.Items, ct);
            var merged = lines
                .GroupBy(l => l.Product.Id)
                .Select(g => new ProductLine(g.First().Product, BoxQuantity.FromUnits(
                    g.Sum(l => l.Quantity.ToUnits(l.Product.UnitsPerBox)), g.First().Product.UnitsPerBox)))
                .ToList();
            StockOperations.EnsureAvailable(StockOrigin.Warehouse, null, merged);

            var consignment = new ConsignmentEvent(request.Name, request.Responsible, request.EventDate ?? clock.UtcNow);
            foreach (var line in lines)
            {
                line.Product.RemoveStock(line.Quantity);
                consignment.Deliver(line.Product, line.Quantity);
            }

            consignments.Add(consignment);
            await unitOfWork.SaveChangesAsync(ct);
            return consignment;
        }, cancellationToken);

        await audit.AuditAsync("consignment_created", "consignments", $"Evento a consignación: {created.Name}",
            nameof(ConsignmentEvent), created.Id.ToString(),
            metadata: created.Items.Select(i => new { i.ProductName, i.DeliveredUnits }), cancellationToken: cancellationToken);
        return created.ToResponse();
    }

    /// <summary>Registra una devolución (evento abierto, sin superar lo pendiente) y reintegra el stock al galpón.</summary>
    public async Task<ConsignmentResponse> RegisterReturnAsync(Guid id, ConsignmentReturnRequest request, CancellationToken cancellationToken = default)
    {
        await returnValidator.ValidateAndThrowAsync(request, cancellationToken);

        var (consignment, units) = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var target = await consignments.GetForUpdateAsync(id, ct)
                ?? throw new KeyNotFoundException($"No existe el evento {id}.");
            var found = await products.GetManyForUpdateAsync([request.ProductId], ct);
            if (!found.TryGetValue(request.ProductId, out var product))
            {
                throw new InvalidOperationException($"El producto {request.ProductId} no existe.");
            }

            int returned = target.RegisterReturn(product.Id, new BoxQuantity(request.Boxes, request.LooseUnits));
            product.AddStock(BoxQuantity.FromUnits(returned, product.UnitsPerBox));
            await unitOfWork.SaveChangesAsync(ct);
            return (target, returned);
        }, cancellationToken);

        await audit.AuditAsync("consignment_return", "consignments", $"Devolución de {units} und en {consignment.Name}",
            nameof(ConsignmentEvent), id.ToString(), metadata: request, cancellationToken: cancellationToken);
        return consignment.ToResponse();
    }

    public async Task<ConsignmentResponse> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var consignment = await consignments.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el evento {id}.");
        consignment.Close(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("consignment_closed", "consignments", $"Evento cerrado: {consignment.Name}",
            nameof(ConsignmentEvent), id.ToString(), consignment.TotalSold, cancellationToken: cancellationToken);
        return consignment.ToResponse();
    }

    public async Task<ConsignmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var consignment = await consignments.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el evento {id}.");
        return consignment.ToResponse();
    }

    public async Task<IReadOnlyList<ConsignmentResponse>> ListAsync(bool? isClosed, CancellationToken cancellationToken = default)
    {
        var list = await consignments.ListAsync(isClosed, cancellationToken);
        return list.Select(c => c.ToResponse()).ToList();
    }

    public async Task<ConsignmentIndicatorsResponse> GetIndicatorsAsync(CancellationToken cancellationToken = default)
    {
        var list = await consignments.ListAsync(null, cancellationToken);
        var items = list.SelectMany(c => c.Items).ToList();
        return new ConsignmentIndicatorsResponse(
            list.Count(c => !c.IsClosed),
            list.Count(c => c.IsClosed),
            list.Sum(c => c.TotalSold),
            list.Where(c => !c.IsClosed).Sum(c => c.TotalSold),
            list.Where(c => c.IsClosed).Sum(c => c.TotalSold),
            items.Sum(i => i.DeliveredUnits),
            items.Sum(i => i.ReturnedUnits),
            items.Sum(i => i.SoldUnits));
    }

    public async Task<byte[]> GetPdfAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var consignment = await consignments.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el evento {id}.");
        return pdf.RenderConsignment(consignment);
    }
}
