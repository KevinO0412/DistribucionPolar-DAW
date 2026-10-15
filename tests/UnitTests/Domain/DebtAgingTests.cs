using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Domain.Services;

namespace UnitTests.Domain;

/// <summary>Envejecimiento de la deuda (FIFO) y política de crédito (observaciones 2 y 10).</summary>
public sealed class DebtAgingTests
{
    private static readonly DateOnly Today = new(2026, 10, 15);

    private static OpenSale Sale(int daysAgo, decimal pending) => new(Today.AddDays(-daysAgo), pending);

    [Fact]
    public void Allocate_AssignsTheDebtToTheNewestInvoices_AndBucketsByAge()
    {
        var aging = DebtAging.Allocate(260m, [Sale(5, 60m), Sale(35, 100m), Sale(70, 50m), Sale(120, 80m)], Today);

        Assert.Equal((60m, 100m, 50m, 50m), (aging.Days0To30, aging.Days31To60, aging.Days61To90, aging.Over90));
        Assert.Equal(120, aging.OldestDays);
        Assert.Equal(200m, aging.AmountOlderThan(30));
    }

    [Theory]
    [InlineData(30, 30, 0)]
    [InlineData(31, 0, 31)]
    public void Allocate_ThirtyDaysIsStillTheFirstBucket(int daysAgo, int firstBucket, int secondBucket)
    {
        var aging = DebtAging.Allocate(daysAgo, [Sale(daysAgo, daysAgo)], Today);

        Assert.Equal((firstBucket, secondBucket), ((int)aging.Days0To30, (int)aging.Days31To60));
    }

    [Fact]
    public void Allocate_DebtWithoutInvoices_IsTheOldest()
    {
        // Ajuste manual de saldo: no hay factura que lo explique.
        var aging = DebtAging.Allocate(100m, [Sale(3, 40m)], Today);

        Assert.Equal((40m, 60m), (aging.Days0To30, aging.Over90));
        Assert.Null(aging.OldestDays);
        Assert.Equal(60m, aging.AmountOlderThan(30));
    }

    [Fact]
    public void Allocate_WithoutDebt_IsEmpty()
    {
        Assert.Same(DebtAgingResult.Empty, DebtAging.Allocate(-20m, [Sale(3, 40m)], Today));
    }

    [Fact]
    public void CreditPolicy_RecentDebtWithinLimit_AllowsCreditSale()
    {
        var client = new Client(1, "V-1", "Bodega", null, null, null);
        client.SetCreditTerms(500m, 30);
        client.AdjustBalances(-100m, 0, 0);

        CreditPolicy.EnsureCanSell(client, [Sale(10, 100m)], Today, 50m);

        var status = CreditPolicy.Evaluate(client, [Sale(10, 100m)], Today);
        Assert.False(status.IsBlocked);
        Assert.Equal(400m, status.Available);
    }

    [Fact]
    public void CreditPolicy_ZeroLimit_OnlyAllowsCash()
    {
        var client = new Client(1, "V-1", "Bodega", null, null, null);
        client.SetCreditTerms(0m, 30);

        CreditPolicy.EnsureCanSell(client, [], Today, 0m);
        Assert.Throws<DomainException>(() => CreditPolicy.EnsureCanSell(client, [], Today, 0.01m));
    }

    [Fact]
    public void Client_RejectsInvalidCreditTerms()
    {
        var client = new Client(1, "V-1", "Bodega", null, null, null);

        Assert.Throws<DomainException>(() => client.SetCreditTerms(-1m, 30));
        Assert.Throws<DomainException>(() => client.SetCreditTerms(10m, 0));
        Assert.Equal((null, Client.DefaultCreditDays), (client.CreditLimit, client.CreditDays));
    }
}
