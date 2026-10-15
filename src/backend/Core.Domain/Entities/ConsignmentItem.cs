using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>Producto entregado en un evento a consignación (cantidades en unidades).</summary>
public sealed class ConsignmentItem
{
    // Constructor para EF Core.
    private ConsignmentItem()
    {
    }

    internal ConsignmentItem(Guid eventId, Product product, int deliveredUnits)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        ProductId = product.Id;
        ProductName = product.Name;
        PriceBox = product.PriceBox;
        PriceUnit = product.PriceUnit;
        UnitsPerBox = product.UnitsPerBox;
        DeliveredUnits = deliveredUnits;
    }

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = default!;

    public decimal PriceBox { get; private set; }

    public decimal PriceUnit { get; private set; }

    public int UnitsPerBox { get; private set; }

    public int DeliveredUnits { get; private set; }

    public int ReturnedUnits { get; private set; }

    public int PendingUnits => DeliveredUnits - ReturnedUnits;

    /// <summary>Vendidas = max(0, entregadas − devueltas).</summary>
    public int SoldUnits => Math.Max(0, DeliveredUnits - ReturnedUnits);

    /// <summary>Monto vendido: cajas vendidas × precio caja + sueltas vendidas × precio unidad.</summary>
    public decimal SoldAmount
    {
        get
        {
            var sold = BoxQuantity.FromUnits(SoldUnits, UnitsPerBox);
            return sold.Boxes * PriceBox + sold.LooseUnits * PriceUnit;
        }
    }

    internal void AddDelivered(int units) => DeliveredUnits += units;

    internal void AddReturned(int units) => ReturnedUnits += units;
}
