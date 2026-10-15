using Core.Domain.Common;
using Core.Domain.Exceptions;
using Core.Domain.Services;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>
/// Producto del catálogo. El stock se guarda siempre en unidades; precio por caja y por unidad son independientes.
/// </summary>
public sealed class Product : BaseEntity
{
    public const int NameMaxLength = 150;
    public const int BrandMaxLength = 80;
    public const int SkuMaxLength = 20;
    public const int EmptyGroupKeyMaxLength = 50;

    // Constructor para EF Core.
    private Product()
    {
    }

    public Product(
        string name,
        string brand,
        string sku,
        Guid categoryId,
        decimal priceBox,
        decimal priceUnit,
        decimal costPrice,
        int unitsPerBox,
        int stockUnits,
        int minStock,
        int maxStock,
        bool isReturnable,
        string? emptyGroupKey,
        bool isGiftEligible = false)
    {
        if (!CorporateSkuGenerator.IsValid(sku))
        {
            throw new DomainException($"El SKU '{sku}' no cumple el formato {CorporateSkuGenerator.SkuFormula}.");
        }

        SKU = sku;
        StockUnits = Guard.NotNegative(stockUnits, "stock");
        IsActive = true;
        Apply(name, brand, categoryId, priceBox, priceUnit, costPrice, unitsPerBox, minStock, maxStock, isReturnable, emptyGroupKey, isGiftEligible);
    }

    public string Name { get; private set; } = default!;

    public string Brand { get; private set; } = default!;

    public string SKU { get; private set; } = default!;

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public decimal PriceBox { get; private set; }

    public decimal PriceUnit { get; private set; }

    public decimal CostPrice { get; private set; }

    public int UnitsPerBox { get; private set; }

    public int StockUnits { get; private set; }

    public int MinStock { get; private set; }

    public int MaxStock { get; private set; }

    public bool IsReturnable { get; private set; }

    public string? EmptyGroupKey { get; private set; }

    /// <summary>Indica si el producto puede entregarse como regalía a clientes.</summary>
    public bool IsGiftEligible { get; private set; }

    public bool IsActive { get; private set; }

    public BoxQuantity Stock => BoxQuantity.FromUnits(StockUnits, UnitsPerBox);

    public bool GeneratesEmptyReturns => EmptyGroupKey is not null;

    public void UpdateDetails(
        string name,
        string brand,
        Guid categoryId,
        decimal priceBox,
        decimal priceUnit,
        decimal costPrice,
        int unitsPerBox,
        int minStock,
        int maxStock,
        bool isReturnable,
        string? emptyGroupKey,
        bool isGiftEligible = false)
    {
        Apply(name, brand, categoryId, priceBox, priceUnit, costPrice, unitsPerBox, minStock, maxStock, isReturnable, emptyGroupKey, isGiftEligible);
        Touch();
    }

    /// <summary>Subtotal de una línea: cajas * precio caja + sueltas * precio unidad.</summary>
    public decimal CalculateSubtotal(BoxQuantity quantity)
    {
        return quantity.Boxes * PriceBox + quantity.LooseUnits * PriceUnit;
    }

    /// <summary>Valor del inventario del producto: unidades * precio unidad.</summary>
    public decimal InventoryValue => StockUnits * PriceUnit;

    public bool HasStock(BoxQuantity quantity) => quantity.ToUnits(UnitsPerBox) <= StockUnits;

    public void AddStock(BoxQuantity quantity)
    {
        int units = quantity.ToUnits(UnitsPerBox);
        if (units <= 0)
        {
            throw new DomainException("La cantidad a ingresar debe ser mayor que 0.");
        }

        StockUnits += units;
        Touch();
    }

    public void RemoveStock(BoxQuantity quantity)
    {
        int units = quantity.ToUnits(UnitsPerBox);
        if (units <= 0)
        {
            throw new DomainException("La cantidad a retirar debe ser mayor que 0.");
        }

        if (units > StockUnits)
        {
            throw new DomainException(
                $"Inventario insuficiente: se solicitan {units} und y hay {StockUnits} und ({Stock}).");
        }

        StockUnits -= units;
        Touch();
    }

    /// <summary>Ajuste manual (solo administración): fija el stock en un valor absoluto.</summary>
    public void SetStock(BoxQuantity quantity)
    {
        StockUnits = Guard.NotNegative(quantity.ToUnits(UnitsPerBox), "stock");
        Touch();
    }

    public StockHealthReport EvaluateHealth()
    {
        return ProductHealthEvaluator.Evaluate(SKU, StockUnits, MinStock, MaxStock);
    }

    private void Apply(
        string name,
        string brand,
        Guid categoryId,
        decimal priceBox,
        decimal priceUnit,
        decimal costPrice,
        int unitsPerBox,
        int minStock,
        int maxStock,
        bool isReturnable,
        string? emptyGroupKey,
        bool isGiftEligible)
    {
        if (categoryId == Guid.Empty)
        {
            throw new DomainException("La categoría es obligatoria.");
        }

        Guard.NotNegative(minStock, "stock mínimo");
        if (maxStock <= minStock)
        {
            throw new DomainException("El stock máximo debe superar al stock mínimo.");
        }

        if (emptyGroupKey is not null && !EmptyReturnGroups.Exists(emptyGroupKey))
        {
            throw new DomainException($"El grupo de vacíos '{emptyGroupKey}' no existe.");
        }

        Name = Guard.RequiredText(name, "nombre del producto", NameMaxLength);
        Brand = Guard.RequiredText(brand, "marca", BrandMaxLength);
        CategoryId = categoryId;
        PriceBox = Guard.Positive(priceBox, "precio por caja");
        PriceUnit = Guard.Positive(priceUnit, "precio por unidad");
        CostPrice = Guard.NotNegative(costPrice, "costo");
        UnitsPerBox = Guard.Positive(unitsPerBox, "unidades por caja");
        MinStock = minStock;
        MaxStock = maxStock;
        IsReturnable = isReturnable;
        EmptyGroupKey = emptyGroupKey;
        IsGiftEligible = isGiftEligible;
    }
}
