using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>Línea de factura con copia de los datos del producto al momento de emitirla.</summary>
public sealed class InvoiceItem
{
    // Constructor para EF Core.
    private InvoiceItem()
    {
    }

    internal InvoiceItem(Guid invoiceId, Product product, BoxQuantity quantity, bool chargeable)
    {
        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        ProductId = product.Id;
        ProductName = product.Name;
        SKU = product.SKU;
        PriceBox = product.PriceBox;
        PriceUnit = product.PriceUnit;
        UnitsPerBox = product.UnitsPerBox;
        EmptyGroupKey = product.EmptyGroupKey;
        Boxes = quantity.Boxes;
        LooseUnits = quantity.LooseUnits;
        TotalUnits = quantity.ToUnits(product.UnitsPerBox);
        Subtotal = chargeable ? product.CalculateSubtotal(quantity) : 0m;
    }

    public Guid Id { get; private set; }

    public Guid InvoiceId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = default!;

    public string SKU { get; private set; } = default!;

    public decimal PriceBox { get; private set; }

    public decimal PriceUnit { get; private set; }

    public int UnitsPerBox { get; private set; }

    public string? EmptyGroupKey { get; private set; }

    public int Boxes { get; private set; }

    public int LooseUnits { get; private set; }

    public int TotalUnits { get; private set; }

    public decimal Subtotal { get; private set; }

    public BoxQuantity Quantity => new(Boxes, LooseUnits);
}
