using Core.Domain.Common;
using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Liquidación ciega de la ruta de un camión: el chofer o vendedor declara el efectivo recaudado y la mercancía que
/// devuelve SIN ver lo esperado; el sistema lo compara con lo que registró (cobros de la ruta e inventario del camión)
/// y guarda las diferencias. Es la planilla de cuadre de la ruta.
/// </summary>
public sealed class TruckSettlement : BaseEntity
{
    public const int ObservationMaxLength = 500;

    private readonly List<TruckSettlementLine> lines = [];

    // Constructor para EF Core.
    private TruckSettlement()
    {
    }

    private TruckSettlement(Truck truck, DateTime tripStartedAt, DateTime settledAt, string? settledBy, string? observation)
    {
        TruckId = truck.Id;
        TruckName = truck.Name;
        TruckPlate = truck.Plate;
        TripStartedAt = tripStartedAt;
        SettledAt = settledAt;
        SettledBy = settledBy;
        Observation = Guard.OptionalText(observation, "observación", ObservationMaxLength);
    }

    public Guid TruckId { get; private set; }

    public string TruckName { get; private set; } = default!;

    public string TruckPlate { get; private set; } = default!;

    /// <summary>Salida a ruta: desde aquí se cuentan los cobros esperados.</summary>
    public DateTime TripStartedAt { get; private set; }

    public DateTime SettledAt { get; private set; }

    public string? SettledBy { get; private set; }

    public string? Observation { get; private set; }

    /// <summary>Cobrado en las facturas de la ruta según el sistema.</summary>
    public decimal ExpectedCash { get; private set; }

    /// <summary>Efectivo que declaró el chofer.</summary>
    public decimal CountedCash { get; private set; }

    /// <summary>Contado − esperado: negativo = falta dinero; positivo = sobra.</summary>
    public decimal CashDifference { get; private set; }

    /// <summary>Valor (a precio por unidad) de la mercancía que el sistema espera de vuelta.</summary>
    public decimal ExpectedMerchandiseValue { get; private set; }

    public decimal CountedMerchandiseValue { get; private set; }

    /// <summary>Contado − esperado, valorizado: negativo = falta mercancía.</summary>
    public decimal MerchandiseDifferenceValue { get; private set; }

    public IReadOnlyCollection<TruckSettlementLine> Lines => lines.AsReadOnly();

    /// <summary>La ruta cuadra: sin diferencia de efectivo ni de mercancía.</summary>
    public bool IsBalanced => CashDifference == 0 && lines.All(l => l.DifferenceUnits == 0);

    /// <summary>
    /// Arma la liquidación: cada producto del camión se compara con lo contado (lo no contado cuenta como 0) y cada
    /// producto contado que el camión no tenía aparece como sobrante. Deja el camión "Liquidado".
    /// </summary>
    /// <param name="truck">Camión en ruta, con su inventario y sus productos.</param>
    /// <param name="expectedCash">Cobros de las facturas de la ruta.</param>
    /// <param name="countedCash">Efectivo declarado.</param>
    /// <param name="countedUnits">Unidades contadas por producto.</param>
    /// <param name="products">Productos contados que no están en el camión (para valorizar sobrantes).</param>
    /// <param name="settledAt">Momento de la liquidación (UTC).</param>
    /// <param name="settledBy">Usuario que liquida.</param>
    /// <param name="observation">Observación opcional.</param>
    public static TruckSettlement Create(
        Truck truck,
        decimal expectedCash,
        decimal countedCash,
        IReadOnlyDictionary<Guid, int> countedUnits,
        IReadOnlyDictionary<Guid, Product> products,
        DateTime settledAt,
        string? settledBy,
        string? observation)
    {
        if (!truck.IsOnRoute || truck.DispatchedAt is null)
        {
            throw new DomainException($"El camión {truck.Name} no está en ruta; solo se liquida un camión que salió a ruta.");
        }

        if (countedCash < 0 || countedUnits.Values.Any(u => u < 0))
        {
            throw new DomainException("Los conteos no pueden ser negativos.");
        }

        var settlement = new TruckSettlement(truck, truck.DispatchedAt.Value, settledAt, settledBy, observation);
        foreach (var line in truck.Stock)
        {
            var product = line.Product ?? (products.TryGetValue(line.ProductId, out var p) ? p : null)
                ?? throw new DomainException($"Falta el producto {line.ProductId} para liquidar.");
            settlement.lines.Add(new TruckSettlementLine(settlement.Id, product, line.StockUnits, countedUnits.GetValueOrDefault(line.ProductId)));
        }

        foreach (var (productId, units) in countedUnits.Where(c => c.Value > 0 && truck.Stock.All(s => s.ProductId != c.Key)))
        {
            var product = products.TryGetValue(productId, out var p) ? p : throw new DomainException($"El producto {productId} no existe.");
            settlement.lines.Add(new TruckSettlementLine(settlement.Id, product, 0, units));
        }

        settlement.ExpectedCash = Math.Round(expectedCash, 2);
        settlement.CountedCash = Math.Round(countedCash, 2);
        settlement.CashDifference = settlement.CountedCash - settlement.ExpectedCash;
        settlement.ExpectedMerchandiseValue = settlement.lines.Sum(l => l.ExpectedValue);
        settlement.CountedMerchandiseValue = settlement.lines.Sum(l => l.CountedValue);
        settlement.MerchandiseDifferenceValue = settlement.CountedMerchandiseValue - settlement.ExpectedMerchandiseValue;

        truck.MarkSettled();
        return settlement;
    }
}

/// <summary>Producto de la planilla de cuadre: lo que el sistema esperaba frente a lo que se contó.</summary>
public sealed class TruckSettlementLine
{
    // Constructor para EF Core.
    private TruckSettlementLine()
    {
    }

    internal TruckSettlementLine(Guid settlementId, Product product, int expectedUnits, int countedUnits)
    {
        SettlementId = settlementId;
        ProductId = product.Id;
        ProductName = product.Name;
        SKU = product.SKU;
        UnitsPerBox = product.UnitsPerBox;
        PriceUnit = product.PriceUnit;
        ExpectedUnits = expectedUnits;
        CountedUnits = countedUnits;
    }

    public Guid SettlementId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = default!;

    public string SKU { get; private set; } = default!;

    public int UnitsPerBox { get; private set; }

    public decimal PriceUnit { get; private set; }

    public int ExpectedUnits { get; private set; }

    public int CountedUnits { get; private set; }

    /// <summary>Contado − esperado: negativo = faltante; positivo = sobrante.</summary>
    public int DifferenceUnits => CountedUnits - ExpectedUnits;

    public decimal ExpectedValue => ExpectedUnits * PriceUnit;

    public decimal CountedValue => CountedUnits * PriceUnit;

    public decimal DifferenceValue => DifferenceUnits * PriceUnit;
}
