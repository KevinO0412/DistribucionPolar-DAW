using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Domain.Services;

namespace UnitTests.Domain;

/// <summary>Reglas agregadas por las observaciones del docente: antigüedad de deuda, crédito y ruta del camión.</summary>
public sealed class DebtAgingTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Allocate_PaymentsSettleTheOldestFirst()
    {
        // Debe 100: facturas de hace 40 días (pendiente 80) y de hace 5 días (pendiente 60).
        var sales = new[] { new PendingSale(Now.AddDays(-40), 80m), new PendingSale(Now.AddDays(-5), 60m) };

        var parts = DebtAging.Allocate(100m, sales, Now);

        // Los 60 más recientes primero; de la factura vieja solo quedan 40.
        Assert.Equal([new AgedAmount(5, 60m), new AgedAmount(40, 40m)], parts);
        Assert.Equal(40m, DebtAging.Overdue(100m, sales, Now, 30));
    }

    [Fact]
    public void Allocate_DebtWithoutInvoices_IsTheOldest()
    {
        var parts = DebtAging.Allocate(50m, [new PendingSale(Now.AddDays(-2), 20m)], Now);

        Assert.Equal([new AgedAmount(2, 20m), new AgedAmount(DebtAging.UnexplainedAge, 30m)], parts);
    }

    [Fact]
    public void Allocate_NoDebt_IsEmpty()
    {
        Assert.Empty(DebtAging.Allocate(0m, [new PendingSale(Now, 10m)], Now));
    }
}

public sealed class CreditControlTests
{
    [Fact]
    public void OverdueDebt_BlocksCreditSales()
    {
        var client = TestData.Client();

        var error = Assert.Throws<DomainException>(() => client.EnsureCreditAvailable(newPending: 10m, overdueDebt: 40m, maxOverdueDays: 30));

        Assert.Contains("más de 30 días", error.Message);
    }

    [Fact]
    public void CashSale_IsAllowedEvenWithOverdueDebt()
    {
        TestData.Client().EnsureCreditAvailable(newPending: 0m, overdueDebt: 40m, maxOverdueDays: 30);
    }

    [Fact]
    public void CreditLimit_BlocksASaleThatExceedsIt()
    {
        var client = TestData.Client();
        client.AdjustBalances(-80m, 0, 0);
        client.SetCreditLimit(100m);

        var error = Assert.Throws<DomainException>(() => client.EnsureCreditAvailable(30m, 0m, 30));

        Assert.Contains("Cobre al menos $10.00", error.Message);
        client.EnsureCreditAvailable(20m, 0m, 30);
    }

    [Fact]
    public void CreditLimit_Zero_MeansCashOnly()
    {
        var client = TestData.Client();
        client.SetCreditLimit(0m);

        Assert.Throws<DomainException>(() => client.EnsureCreditAvailable(0.01m, 0m, 30));
        Assert.Throws<DomainException>(() => client.SetCreditLimit(-1m));
    }
}

public sealed class TruckRouteTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Route_GoesLoadingThenOnRouteThenSettled()
    {
        var truck = TestData.Truck();
        truck.AddStock(Guid.NewGuid(), 36);

        truck.MarkLoading();
        Assert.Equal(Truck.StatusLoading, truck.Status);
        truck.Dispatch(Now);
        Assert.Equal((Truck.StatusOnRoute, Now), (truck.Status, truck.DispatchedAt));

        var expected = truck.Settle();

        Assert.Equal(36, expected.Single().Units);
        Assert.Equal(Truck.StatusSettled, truck.Status);
        Assert.Empty(truck.Stock);
    }

    [Fact]
    public void Dispatch_EmptyTruckOrAlreadyOnRoute_IsRejected()
    {
        var truck = TestData.Truck();
        Assert.Throws<DomainException>(() => truck.Dispatch(Now));

        truck.AddStock(Guid.NewGuid(), 10);
        truck.Dispatch(Now);
        Assert.Throws<DomainException>(() => truck.Dispatch(Now));
    }

    [Fact]
    public void LoadingWhileOnRoute_KeepsTheRouteOpen()
    {
        var truck = TestData.Truck();
        truck.AddStock(Guid.NewGuid(), 10);
        truck.Dispatch(Now);

        truck.MarkLoading();

        Assert.True(truck.IsOnRoute);
    }

    [Fact]
    public void Settle_TruckNotOnRoute_IsRejected()
    {
        Assert.Throws<DomainException>(() => TestData.Truck().Settle());
    }

    [Fact]
    public void Settlement_ComparesDeclaredAgainstExpected()
    {
        // Debía traer 2 cajas de Pilsen (72 und) y $40 de abonos; declara 60 und y $35.
        var pilsen = TestData.Pilsen();
        var truck = TestData.Truck();
        truck.AddStock(pilsen.Id, 72);
        truck.Dispatch(Now.AddHours(-8));
        var expected = truck.Settle();

        var settlement = RouteSettlement.Create(
            truck, Now.AddHours(-8), new Dictionary<Guid, Product> { [pilsen.Id] = pilsen }, expected,
            new Dictionary<Guid, int> { [pilsen.Id] = 60 }, countedCash: 35m, expectedCash: 40m, salesTotal: 230m,
            invoicesCount: 3, Now, "empleado", null);

        Assert.Equal(-5m, settlement.CashDifference);
        var line = settlement.Lines.Single();
        Assert.Equal((72, 60, -12), (line.ExpectedUnits, line.CountedUnits, line.DifferenceUnits));
        Assert.Equal(7.68m, settlement.MissingGoodsValue);           // 12 × $0.64
        Assert.False(settlement.IsBalanced);
    }

    [Fact]
    public void Settlement_WithEverythingDeclared_IsBalanced()
    {
        var pilsen = TestData.Pilsen();
        var truck = TestData.Truck();
        truck.AddStock(pilsen.Id, 36);
        truck.Dispatch(Now);

        var settlement = RouteSettlement.Create(
            truck, Now, new Dictionary<Guid, Product> { [pilsen.Id] = pilsen }, truck.Settle(),
            new Dictionary<Guid, int> { [pilsen.Id] = 36 }, 20m, 20m, 0m, 0, Now, null, null);

        Assert.True(settlement.IsBalanced);
        Assert.Throws<DomainException>(() => RouteSettlement.Create(
            truck, Now, new Dictionary<Guid, Product>(), [], new Dictionary<Guid, int>(), -1m, 0m, 0m, 0, Now, null, null));
    }
}
