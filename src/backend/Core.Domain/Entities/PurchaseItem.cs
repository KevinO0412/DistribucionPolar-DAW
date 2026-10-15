using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>Línea de compra con copia de los datos del producto al momento de registrarla.</summary>
public sealed class PurchaseItem
{
    // Constructor para EF Core.
    private PurchaseItem()
    {
    }

    internal PurchaseItem(Guid purchaseId, Product product, BoxQuantity quantity)
    {
        Id = Guid.NewGuid();
        PurchaseId = purchaseId;
        ProductId = product.Id;
        ProductName = product.Name;
        PriceBox = product.PriceBox;
        PriceUnit = product.PriceUnit;
        UnitsPerBox = product.UnitsPerBox;
        Boxes = quantity.Boxes;
        LooseUnits = quantity.LooseUnits;
        TotalUnits = quantity.ToUnits(product.UnitsPerBox);
        Subtotal = product.CalculateSubtotal(quantity);
    }

    public Guid Id { get; private set; }

    public Guid PurchaseId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = default!;

    public decimal PriceBox { get; private set; }

    public decimal PriceUnit { get; private set; }

    public int UnitsPerBox { get; private set; }

    public int Boxes { get; private set; }

    public int LooseUnits { get; private set; }

    public int TotalUnits { get; private set; }

    public decimal Subtotal { get; private set; }
}
