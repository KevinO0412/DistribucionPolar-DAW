using Core.Domain.Common;
using Core.Domain.Enums;

namespace Core.Domain.Entities;

/// <summary>
/// Registro histórico de cada entrada de mercancía a un camión: qué se cargó, de dónde vino
/// y cuánto había antes y después.
/// </summary>
public sealed class TruckLoad : BaseEntity
{
    public const int ObservationMaxLength = 500;

    // Constructor para EF Core.
    private TruckLoad()
    {
    }

    public TruckLoad(
        Truck truck,
        Product product,
        int totalUnits,
        int stockBefore,
        int stockAfter,
        TruckLoadSource source,
        Guid? sourceTruckId,
        string? observation,
        string? username)
    {
        TruckId = truck.Id;
        TruckName = truck.Name;
        TruckPlate = truck.Plate;
        ProductId = product.Id;
        ProductName = product.Name;
        UnitsPerBox = product.UnitsPerBox;
        TotalUnits = Guard.Positive(totalUnits, "cantidad cargada");
        StockBefore = stockBefore;
        StockAfter = stockAfter;
        Source = source;
        SourceTruckId = sourceTruckId;
        Observation = Guard.OptionalText(observation, "observación", ObservationMaxLength);
        Username = username;
    }

    public Guid TruckId { get; private set; }

    public string TruckName { get; private set; } = default!;

    public string TruckPlate { get; private set; } = default!;

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = default!;

    public int UnitsPerBox { get; private set; }

    public int TotalUnits { get; private set; }

    /// <summary>Unidades del producto en el camión antes de la carga.</summary>
    public int StockBefore { get; private set; }

    /// <summary>Unidades del producto en el camión después de la carga.</summary>
    public int StockAfter { get; private set; }

    public TruckLoadSource Source { get; private set; }

    public Guid? SourceTruckId { get; private set; }

    public string? Observation { get; private set; }

    public string? Username { get; private set; }
}
