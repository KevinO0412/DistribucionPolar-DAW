using Core.Application.DTOs;
using Core.Domain.Common;
using Core.Domain.Entities;

namespace Core.Application.Mappings;

internal static class Mappings
{
    public static CategoryResponse ToResponse(this Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.Deposit,
        category.CreatedAt,
        category.LastModifiedAt);

    public static ProductResponse ToResponse(this Product product, Category? category = null)
    {
        category ??= product.Category;
        var stock = product.Stock;

        return new ProductResponse(
            product.Id,
            product.SKU,
            product.Name,
            product.Brand,
            product.CategoryId,
            category?.Name,
            category?.Deposit,
            product.PriceBox,
            product.PriceUnit,
            product.CostPrice,
            product.UnitsPerBox,
            product.StockUnits,
            stock.Boxes,
            stock.LooseUnits,
            stock.ToString(),
            product.MinStock,
            product.MaxStock,
            product.InventoryValue,
            product.IsReturnable,
            product.EmptyGroupKey,
            product.EmptyGroupKey is null ? null : EmptyReturnGroups.GetName(product.EmptyGroupKey),
            product.IsGiftEligible,
            product.IsActive,
            product.CreatedAt,
            product.LastModifiedAt);
    }
}
