using Core.Domain.Entities;
using Core.Domain.Exceptions;

namespace UnitTests.Domain;

/// <summary>Liquidación ciega (observación 1): esperado frente a contado, en efectivo y mercancía.</summary>
public sealed class TruckSettlementTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 22, 0, 0, DateTimeKind.Utc);
    private readonly Product beer = TestData.Pilsen(stockUnits: 1000);
    private readonly Product flour = TestData.Coffee(stockUnits: 1000);

    private Truck TruckOnRoute()
    {
        var truck = new Truck("NPR", "AB123CD");
        truck.AddStock(beer.Id, 72);
        truck.AddStock(flour.Id, 20);
        truck.Dispatch(Now.AddHours(-10));
        return truck;
    }

    private Dictionary<Guid, Product> Products() => new() { [beer.Id] = beer, [flour.Id] = flour };

    [Fact]
    public void Create_WhenEverythingMatches_IsBalanced_AndSettlesTheTruck()
    {
        var truck = TruckOnRoute();

        var settlement = TruckSettlement.Create(truck, 350m, 350m, new Dictionary<Guid, int> { [beer.Id] = 72, [flour.Id] = 20 },
            Products(), Now, "empleado", null);

        Assert.True(settlement.IsBalanced);
        Assert.Equal(Truck.StatusSettled, truck.Status);
        Assert.Equal(Now.AddHours(-10), settlement.TripStartedAt);
    }

    [Fact]
    public void Create_ComputesCashAndMerchandiseDiscrepancies()
    {
        // Arrange: faltan $20 y 6 und de cerveza; no contó la harina (20 und).
        var truck = TruckOnRoute();

        // Act
        var settlement = TruckSettlement.Create(truck, 350m, 330m, new Dictionary<Guid, int> { [beer.Id] = 66 }, Products(), Now, "empleado", "Faltó una caja");

        // Assert
        Assert.Equal(-20m, settlement.CashDifference);
        var beerLine = settlement.Lines.Single(l => l.ProductId == beer.Id);
        var flourLine = settlement.Lines.Single(l => l.ProductId == flour.Id);
        Assert.Equal((72, 66, -6), (beerLine.ExpectedUnits, beerLine.CountedUnits, beerLine.DifferenceUnits));
        Assert.Equal((20, 0, -20), (flourLine.ExpectedUnits, flourLine.CountedUnits, flourLine.DifferenceUnits));
        Assert.Equal(-6 * beer.PriceUnit - 20 * flour.PriceUnit, settlement.MerchandiseDifferenceValue);
        Assert.False(settlement.IsBalanced);
    }

    [Fact]
    public void Create_ProductNotOnTheTruck_IsASurplus()
    {
        var truck = new Truck("NPR", "AB123CD");
        truck.AddStock(beer.Id, 36);
        truck.Dispatch(Now.AddHours(-5));

        var settlement = TruckSettlement.Create(truck, 0m, 0m, new Dictionary<Guid, int> { [beer.Id] = 36, [flour.Id] = 4 }, Products(), Now, null, null);

        var surplus = settlement.Lines.Single(l => l.ProductId == flour.Id);
        Assert.Equal((0, 4, 4), (surplus.ExpectedUnits, surplus.CountedUnits, surplus.DifferenceUnits));
    }

    [Fact]
    public void Create_TruckNotOnRoute_IsRejected()
    {
        var truck = new Truck("NPR", "AB123CD");
        truck.AddStock(beer.Id, 36);

        Assert.Throws<DomainException>(() => TruckSettlement.Create(truck, 0m, 0m, new Dictionary<Guid, int>(), Products(), Now, null, null));
        Assert.Equal(Truck.StatusInWarehouse, truck.Status);
    }

    [Fact]
    public void Create_NegativeCount_IsRejected()
    {
        var truck = TruckOnRoute();

        Assert.Throws<DomainException>(() => TruckSettlement.Create(truck, 0m, -1m, new Dictionary<Guid, int>(), Products(), Now, null, null));
        Assert.True(truck.IsOnRoute);
    }
}
