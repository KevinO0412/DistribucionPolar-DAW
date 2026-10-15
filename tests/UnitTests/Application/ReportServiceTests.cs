using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Services;

namespace UnitTests.Application;

/// <summary>KPI del dashboard calculados sobre datos simulados de IReportQueries (sin base de datos).</summary>
public sealed class ReportServiceTests
{
    private static readonly DateOnly Today = new(2026, 10, 15);
    private static readonly Guid Food = Guid.NewGuid();
    private static readonly Guid Beverages = Guid.NewGuid();
    private static readonly Guid TruckId = Guid.NewGuid();

    private readonly Mock<IReportQueries> mockReports = new();
    private readonly Mock<IClock> mockClock = new();
    private readonly ReportService service;

    public ReportServiceTests()
    {
        // Caracas: UTC−4 todo el año.
        mockClock.Setup(c => c.Today).Returns(Today);
        mockClock.Setup(c => c.GetDayRangeUtc(It.IsAny<DateOnly>())).Returns<DateOnly>(d =>
            (d.ToDateTime(new TimeOnly(4, 0), DateTimeKind.Utc), d.AddDays(1).ToDateTime(new TimeOnly(4, 0), DateTimeKind.Utc)));
        mockClock.Setup(c => c.ToLocal(It.IsAny<DateTime>())).Returns<DateTime>(utc => utc.AddHours(-4));

        service = new ReportService(mockReports.Object, new CreditPolicy(), Mock.Of<IConsignmentService>(), Mock.Of<IAuditLogRepository>(), mockClock.Object);
    }

    private static DashboardProductRow Harina(int warehouse, int truck = 0) =>
        new(Guid.NewGuid(), "ALI-HAR-0001", "HARINA PAN", Food, "Alimentos", 0.95m, 14.80m, 20, warehouse, truck, 100, 800);

    private static DashboardSaleLine Line(Guid invoiceId, DateTime issuedUtc, Guid productId, int units, decimal subtotal, bool gift = false) =>
        new(invoiceId, issuedUtc, gift, TruckId, productId, "ALI-HAR-0001", "HARINA PAN", Food, "Alimentos", units, subtotal);

    private void Returns(DashboardData data) =>
        mockReports.Setup(r => r.GetDashboardDataAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

    private static DashboardData Data(
        IReadOnlyList<DashboardProductRow>? products = null, IReadOnlyList<DashboardSaleLine>? lines = null,
        IReadOnlyList<DashboardTruckRow>? trucks = null, IReadOnlyList<DebtorRow>? debtors = null) =>
        new(products ?? [], lines ?? [], new DashboardCollections(30m, 18m), trucks ?? [], debtors ?? []);

    [Fact]
    public async Task GetDashboardAsync_WithoutDates_UsesTheLast30LocalDays()
    {
        Returns(Data());

        var result = await service.GetDashboardAsync(null, null, null);

        Assert.Equal(new DateOnly(2026, 9, 16), result.From);
        Assert.Equal(Today, result.To);
        Assert.Equal(30, result.SalesByDay.Count);
        mockReports.Verify(r => r.GetDashboardDataAsync(
            new DateTime(2026, 9, 16, 4, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 16, 4, 0, 0, DateTimeKind.Utc),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDashboardAsync_InventoryValuation_ComparesCostWithSalePrice()
    {
        // 200 und en galpón + 40 en camión: costo 14.80 / 20 = 0.74 por und; venta 0.95 por und.
        Returns(Data([Harina(200, 40)]));

        var inventory = (await service.GetDashboardAsync(Today, Today, null)).Inventory;

        Assert.Equal(240, inventory.StockUnits);
        Assert.Equal(177.60m, inventory.CostValue);
        Assert.Equal(228.00m, inventory.SaleValue);
        Assert.Equal(50.40m, inventory.GrossMargin);
        Assert.Equal(22.1m, inventory.GrossMarginPercent);
    }

    [Fact]
    public async Task GetDashboardAsync_SalesExcludeGiftsFromMoney_ButCountTheirUnits()
    {
        var product = Harina(200);
        var sale = Guid.NewGuid();
        var gift = Guid.NewGuid();
        var at = new DateTime(2026, 10, 15, 14, 0, 0, DateTimeKind.Utc);
        Returns(Data([product], [Line(sale, at, product.Id, 20, 18.50m), Line(sale, at, product.Id, 5, 4.75m), Line(gift, at, product.Id, 10, 0m, gift: true)]));

        var sales = (await service.GetDashboardAsync(Today, Today, null)).Sales;

        Assert.Equal(23.25m, sales.SalesTotal);
        Assert.Equal(1, sales.InvoicesCount);
        Assert.Equal(1, sales.GiftsCount);
        Assert.Equal(35, sales.UnitsSold);
        Assert.Equal(23.25m, sales.AverageTicket);
        Assert.Equal(0.18m, sales.RotationIndex);              // 35 / 200
        Assert.Equal((30m, 18m), (sales.CollectedOnInvoices, sales.CreditGenerated));
    }

    [Fact]
    public async Task GetDashboardAsync_GroupsSalesByLocalDay()
    {
        var product = Harina(100);
        // 02:00 UTC del 15 = 22:00 del 14 en Caracas.
        Returns(Data([product], [
            Line(Guid.NewGuid(), new DateTime(2026, 10, 15, 2, 0, 0, DateTimeKind.Utc), product.Id, 20, 18.50m),
            Line(Guid.NewGuid(), new DateTime(2026, 10, 15, 15, 0, 0, DateTimeKind.Utc), product.Id, 40, 37.00m)]));

        var days = (await service.GetDashboardAsync(new DateOnly(2026, 10, 14), Today, null)).SalesByDay;

        Assert.Equal([new SalesPoint(new DateOnly(2026, 10, 14), 18.50m, 1, 20), new SalesPoint(Today, 37.00m, 1, 40)], days);
    }

    [Fact]
    public async Task GetDashboardAsync_FlagsOutOfStockAndCriticalProducts_MostUrgentFirst()
    {
        Returns(Data([Harina(500), Harina(60), Harina(0), Harina(900)]));

        var result = await service.GetDashboardAsync(Today, Today, null);

        Assert.Equal(["OutOfStock", "CriticalRisk", "Excessive"], result.CriticalProducts.Select(p => p.Status));
        Assert.Equal(1, result.StockAlerts.Single(a => a.Status == "Optimal").Count);
        Assert.Equal(4, result.StockAlerts.Sum(a => a.Count));
    }

    [Fact]
    public async Task GetDashboardAsync_TopProducts_ComputesRotationAgainstCurrentStock()
    {
        var product = Harina(80, 20);
        Returns(Data([product], [Line(Guid.NewGuid(), new DateTime(2026, 10, 15, 14, 0, 0, DateTimeKind.Utc), product.Id, 150, 142.50m)]));

        var top = (await service.GetDashboardAsync(Today, Today, null)).TopProducts.Single();

        Assert.Equal(150, top.UnitsSold);
        Assert.Equal(100, top.StockUnits);
        Assert.Equal(1.5m, top.RotationIndex);
    }

    [Fact]
    public async Task GetDashboardAsync_FleetOccupancy_IsLoadedBoxesOverCapacity()
    {
        var product = Harina(100);
        Returns(Data(
            [product],
            [Line(Guid.NewGuid(), new DateTime(2026, 10, 15, 14, 0, 0, DateTimeKind.Utc), product.Id, 20, 18.50m)],
            [new DashboardTruckRow(TruckId, "FVR", "AA200AA", 500, 125m, 2500, 900m), new DashboardTruckRow(Guid.NewGuid(), "F-350", "AA500AA", 150, 0m, 0, 0m)]));

        var fleet = (await service.GetDashboardAsync(Today, Today, null)).Fleet;

        Assert.Equal(650, fleet.CapacityBoxes);
        Assert.Equal(19.2m, fleet.OccupancyPercent);
        var fvr = fleet.Trucks.Single(t => t.Name == "FVR");
        Assert.Equal((25.0m, 18.50m, 1), (fvr.OccupancyPercent, fvr.SalesTotal, fvr.Invoices));
    }

    [Fact]
    public async Task GetDashboardAsync_DebtAging_GroupsDebtByAgeAndFlagsOverdueClients()
    {
        // Arrange: hoy es el 15/10 a las 12:00.
        var now = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
        mockClock.Setup(c => c.UtcNow).Returns(now);
        Returns(Data(debtors:
        [
            new DebtorRow(Guid.NewGuid(), 7, "Bodega La Esquina", 100m, 150m,
                [new Core.Domain.Services.PendingSale(now.AddDays(-45), 70m), new Core.Domain.Services.PendingSale(now.AddDays(-3), 60m)]),
            new DebtorRow(Guid.NewGuid(), 9, "Abasto Mary", 20m, null, [new Core.Domain.Services.PendingSale(now.AddDays(-10), 20m)]),
        ]));

        // Act
        var aging = (await service.GetDashboardAsync(Today, Today, null)).DebtAging;

        // Assert: 60 recientes + 40 viejos de La Esquina; 20 de Mary en 8–15 días.
        Assert.Equal((120m, 2, 30, 40m), (aging.TotalDebt, aging.Debtors, aging.MaxOverdueDays, aging.OverdueDebt));
        Assert.Equal([60m, 20m, 0m, 40m], aging.Buckets.Select(b => b.Amount));
        var top = Assert.Single(aging.TopDebtors);
        Assert.Equal(("007", 40m, 45, 150m), (top.Code, top.OverdueDebt, top.OldestDays, top.CreditLimit));
    }

    [Fact]
    public async Task GetDashboardAsync_PassesTheCategoryFilterToTheQuery()
    {
        Returns(Data());

        var result = await service.GetDashboardAsync(Today, Today, Beverages);

        Assert.Equal(Beverages, result.CategoryId);
        mockReports.Verify(r => r.GetDashboardDataAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), Beverages, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDashboardAsync_StartAfterEnd_IsRejectedWithoutQuerying()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetDashboardAsync(Today, Today.AddDays(-1), null));
        mockReports.Verify(r => r.GetDashboardDataAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetDashboardAsync_RangeLongerThanAYear_IsRejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetDashboardAsync(Today.AddDays(-400), Today, null));
    }
}
