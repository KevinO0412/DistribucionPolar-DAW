using Core.Domain.Common;
using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Liquidación ciega de ruta: al volver, el chofer declara el dinero recaudado y la mercancía que trae sin ver lo
/// esperado. El sistema compara con lo esperado (abonos cobrados en las facturas del camión desde su salida y el
/// inventario que debía traer) y deja la planilla de cuadre con las diferencias.
/// </summary>
public sealed class RouteSettlement : BaseEntity
{
    public const int ObservationMaxLength = 500;

    private readonly List<RouteSettlementLine> lines = [];

    // Constructor para EF Core.
    private RouteSettlement()
    {
    }

    private RouteSettlement(Truck truck, DateTime dispatchedAt, DateTime settledAt, string? settledBy, string? observation)
    {
        TruckId = truck.Id;
        TruckName = truck.Name;
        TruckPlate = truck.Plate;
        DispatchedAt = dispatchedAt;
        SettledAt = settledAt;
        SettledBy = settledBy;
        Observation = Guard.OptionalText(observation, "observación", ObservationMaxLength);
    }

    public Guid TruckId { get; private set; }

    public string TruckName { get; private set; } = default!;

    public string TruckPlate { get; private set; } = default!;

    public DateTime DispatchedAt { get; private set; }

    public DateTime SettledAt { get; private set; }

    public string? SettledBy { get; private set; }

    public string? Observation { get; private set; }

    /// <summary>Facturas de venta (no anuladas) emitidas desde el camión durante la ruta.</summary>
    public int InvoicesCount { get; private set; }

    public decimal SalesTotal { get; private set; }

    /// <summary>Dinero que debía traer: abonos cobrados en esas facturas.</summary>
    public decimal ExpectedCash { get; private set; }

    /// <summary>Dinero que declaró el chofer.</summary>
    public decimal CountedCash { get; private set; }

    /// <summary>Declarado − esperado. Negativo = faltante; positivo = sobrante.</summary>
    public decimal CashDifference { get; private set; }

    /// <summary>Valor de la mercancía faltante (diferencias negativas × precio por unidad).</summary>
    public decimal MissingGoodsValue { get; private set; }

    public bool IsBalanced => CashDifference == 0 && lines.All(l => l.DifferenceUnits == 0);

    public IReadOnlyCollection<RouteSettlementLine> Lines => lines.AsReadOnly();

    /// <summary>
    /// Arma la planilla. <paramref name="expected"/> es lo que el camión debía traer (unidades por producto) y
    /// <paramref name="counted"/> lo que declaró el chofer; los productos de ambas listas quedan en la planilla.
    /// </summary>
    public static RouteSettlement Create(
        Truck truck,
        DateTime dispatchedAt,
        IReadOnlyDictionary<Guid, Product> products,
        IReadOnlyList<(Guid ProductId, int Units)> expected,
        IReadOnlyDictionary<Guid, int> counted,
        decimal countedCash,
        decimal expectedCash,
        decimal salesTotal,
        int invoicesCount,
        DateTime settledAt,
        string? settledBy,
        string? observation)
    {
        if (countedCash < 0)
        {
            throw new DomainException("El dinero declarado no puede ser negativo.");
        }

        if (counted.Values.Any(u => u < 0))
        {
            throw new DomainException("Las cantidades devueltas no pueden ser negativas.");
        }

        var settlement = new RouteSettlement(truck, dispatchedAt, settledAt, settledBy, observation)
        {
            InvoicesCount = invoicesCount,
            SalesTotal = salesTotal,
            ExpectedCash = expectedCash,
            CountedCash = decimal.Round(countedCash, 2),
        };
        settlement.CashDifference = settlement.CountedCash - expectedCash;

        var expectedUnits = expected.GroupBy(e => e.ProductId).ToDictionary(g => g.Key, g => g.Sum(e => e.Units));
        foreach (var productId in expectedUnits.Keys.Union(counted.Keys).Distinct())
        {
            if (!products.TryGetValue(productId, out var product))
            {
                throw new DomainException($"El producto {productId} no existe.");
            }

            settlement.lines.Add(new RouteSettlementLine(
                settlement.Id, product, expectedUnits.GetValueOrDefault(productId), counted.GetValueOrDefault(productId)));
        }

        settlement.MissingGoodsValue = settlement.lines.Where(l => l.DifferenceUnits < 0).Sum(l => -l.DifferenceValue);
        return settlement;
    }
}

/// <summary>Producto de la planilla de cuadre: esperado frente a contado.</summary>
public sealed class RouteSettlementLine
{
    // Constructor para EF Core.
    private RouteSettlementLine()
    {
    }

    internal RouteSettlementLine(Guid settlementId, Product product, int expectedUnits, int countedUnits)
    {
        Id = Guid.NewGuid();
        SettlementId = settlementId;
        ProductId = product.Id;
        ProductName = product.Name;
        UnitsPerBox = product.UnitsPerBox;
        PriceUnit = product.PriceUnit;
        ExpectedUnits = expectedUnits;
        CountedUnits = countedUnits;
        DifferenceUnits = countedUnits - expectedUnits;
        DifferenceValue = DifferenceUnits * product.PriceUnit;
    }

    public Guid Id { get; private set; }

    public Guid SettlementId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = default!;

    public int UnitsPerBox { get; private set; }

    public decimal PriceUnit { get; private set; }

    public int ExpectedUnits { get; private set; }

    public int CountedUnits { get; private set; }

    /// <summary>Contado − esperado, en unidades. Negativo = faltante.</summary>
    public int DifferenceUnits { get; private set; }

    public decimal DifferenceValue { get; private set; }
}
