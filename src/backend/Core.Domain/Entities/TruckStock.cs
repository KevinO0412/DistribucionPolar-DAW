using Core.Domain.Common;

namespace Core.Domain.Entities;

/// <summary>Existencia de un producto dentro de un camión (en unidades).</summary>
public sealed class TruckStock
{
    // Constructor para EF Core.
    private TruckStock()
    {
    }

    internal TruckStock(Guid truckId, Guid productId, int units)
    {
        TruckId = truckId;
        ProductId = Guard.NotEmpty(productId, "producto");
        StockUnits = Guard.Positive(units, "cantidad");
    }

    public Guid TruckId { get; private set; }

    public Guid ProductId { get; private set; }

    public Product? Product { get; private set; }

    public int StockUnits { get; private set; }

    internal void Increase(int units) => StockUnits += units;

    internal void Decrease(int units) => StockUnits -= units;

    internal void Set(int units) => StockUnits = units;
}
