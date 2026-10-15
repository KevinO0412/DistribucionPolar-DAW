using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>
/// Base de las salidas de inventario que no son ventas (averías y consumos internos):
/// producto, cantidad, origen (galpón o camión) y valor estimado = subtotal a precios actuales.
/// </summary>
public abstract class StockWithdrawal : BaseEntity
{
    public const int ObservationMaxLength = 500;

    protected StockWithdrawal()
    {
    }

    protected StockWithdrawal(Product product, BoxQuantity quantity, StockOrigin origin, Truck? truck, string? observation, DateTime occurredAt)
    {
        if (quantity.IsEmpty)
        {
            throw new DomainException("Indique cajas o unidades.");
        }

        Origin = Enum.IsDefined(origin) ? origin : throw new DomainException("Origen inválido.");
        if (origin == StockOrigin.Truck)
        {
            if (truck is null)
            {
                throw new DomainException("El camión es obligatorio cuando el origen es un camión.");
            }

            TruckId = truck.Id;
            TruckName = truck.Name;
        }

        ProductId = product.Id;
        ProductName = product.Name;
        Boxes = quantity.Boxes;
        LooseUnits = quantity.LooseUnits;
        TotalUnits = quantity.ToUnits(product.UnitsPerBox);
        EstimatedValue = product.CalculateSubtotal(quantity);
        Observation = Guard.OptionalText(observation, "observación", ObservationMaxLength);
        OccurredAt = occurredAt;
    }

    public DateTime OccurredAt { get; private set; }

    public StockOrigin Origin { get; private set; }

    public Guid? TruckId { get; private set; }

    public string? TruckName { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = default!;

    public int Boxes { get; private set; }

    public int LooseUnits { get; private set; }

    public int TotalUnits { get; private set; }

    /// <summary>Valor estimado: cajas × precio caja + sueltas × precio unidad.</summary>
    public decimal EstimatedValue { get; private set; }

    public string? Observation { get; private set; }
}
