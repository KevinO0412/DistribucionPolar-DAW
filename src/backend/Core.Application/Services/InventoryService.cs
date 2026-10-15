using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.ValueObjects;
using FluentValidation;

namespace Core.Application.Services;

public sealed class InventoryService(
    IProductRepository products,
    IReportQueries reports,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    IValidator<AdjustWarehouseStockRequest> adjustValidator) : IInventoryService
{
    /// <summary>Ajuste manual del galpón (solo Admin): fija el stock absoluto; motivo obligatorio.</summary>
    public async Task<ProductResponse> AdjustWarehouseStockAsync(
        Guid productId, AdjustWarehouseStockRequest request, CancellationToken cancellationToken = default)
    {
        await adjustValidator.ValidateAndThrowAsync(request, cancellationToken);

        var product = await products.GetForUpdateAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el producto {productId}.");
        int before = product.StockUnits;

        product.SetStock(new BoxQuantity(request.Boxes, request.LooseUnits));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("warehouse_stock_adjusted", "inventory",
            $"Ajuste manual de {product.Name}: {before} und → {product.StockUnits} und. Motivo: {request.Reason}",
            "Product", product.Id.ToString(), before: new { StockUnits = before }, after: new { product.StockUnits },
            metadata: new { request.Reason }, cancellationToken: cancellationToken);

        var reloaded = await products.GetByIdAsync(productId, cancellationToken);
        return reloaded!.ToResponse();
    }

    public Task<InventorySummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default) =>
        reports.GetInventorySummaryAsync(cancellationToken);

    public async Task<ProductLocationResponse> GetProductLocationAsync(Guid productId, CancellationToken cancellationToken = default) =>
        await reports.GetProductLocationAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el producto {productId}.");
}
