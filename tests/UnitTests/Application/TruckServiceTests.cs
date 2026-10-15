using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Application.Validators;
using Core.Domain.Entities;
using Core.Domain.Exceptions;

namespace UnitTests.Application;

/// <summary>Carga de camiones desde el galpón (modos Agregar y Meta), con repositorios simulados.</summary>
public sealed class TruckServiceTests
{
    private readonly Mock<ITruckRepository> mockTrucks = new();
    private readonly Mock<IProductRepository> mockProducts = new();
    private readonly Mock<IUnitOfWork> mockUnitOfWork = TestDoubles.UnitOfWork();
    private readonly Mock<IAuditService> mockAudit = new();
    private readonly Mock<ICurrentUser> mockCurrentUser = new();
    private readonly Mock<IClock> mockClock = new();
    private readonly Dictionary<Guid, TruckTripSales> tripSales = [];
    private readonly TruckService service;
    private readonly Truck truck = new("815-CARGO", "A91AT4L");
    private readonly Product beer = TestDoubles.PolarPilsen(Guid.NewGuid(), stockUnits: 360);

    public TruckServiceTests()
    {
        mockCurrentUser.Setup(u => u.Username).Returns("empleado");
        service = new TruckService(
            mockTrucks.Object, mockProducts.Object, mockUnitOfWork.Object, mockAudit.Object, mockCurrentUser.Object,
            mockClock.Object, new TruckRequestValidator(), new LoadTruckRequestValidator(),
            new TransferFromTruckRequestValidator(), new AdjustStockRequestValidator());

        mockTrucks.Setup(r => r.GetForUpdateAsync(truck.Id, It.IsAny<CancellationToken>())).ReturnsAsync(truck);
        mockTrucks.Setup(r => r.GetByIdAsync(truck.Id, It.IsAny<CancellationToken>())).ReturnsAsync(truck);
        mockProducts.Setup(r => r.GetForUpdateAsync(beer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(beer);
        mockClock.Setup(c => c.UtcNow).Returns(Now);
        mockClock.Setup(c => c.Today).Returns(new DateOnly(2026, 10, 15));
        mockClock.Setup(c => c.GetDayRangeUtc(It.IsAny<DateOnly>())).Returns((DateOnly d) =>
            (d.ToDateTime(new TimeOnly(4, 0), DateTimeKind.Utc), d.AddDays(1).ToDateTime(new TimeOnly(4, 0), DateTimeKind.Utc)));
        mockTrucks.Setup(r => r.GetTripSalesAsync(It.IsAny<IReadOnlyDictionary<Guid, DateTime>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => tripSales);
    }

    private static readonly DateTime Now = new(2026, 10, 15, 13, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task LoadThenDispatch_GoesFromLoadingToOnRoute_AndCountsTheTripSales()
    {
        // Arrange
        Assert.Equal(Truck.StatusInWarehouse, truck.Status);
        await service.LoadAsync(truck.Id, new LoadTruckRequest(beer.Id, TruckLoadMode.Add, 2, 0, null));
        Assert.Equal(Truck.StatusLoading, truck.Status);
        tripSales[truck.Id] = new TruckTripSales(3, 420m, 300m);

        // Act
        var dispatched = await service.DispatchAsync(truck.Id);

        // Assert: las ventas se cuentan desde la salida.
        Assert.Equal((Truck.StatusOnRoute, Now), (dispatched.Status, dispatched.DispatchedAt));
        Assert.Equal((Now, 3, 420m, 300m), (dispatched.TripFrom, dispatched.TripInvoices, dispatched.TripSales, dispatched.TripCollected));
        mockTrucks.Verify(r => r.GetTripSalesAsync(It.Is<IReadOnlyDictionary<Guid, DateTime>>(d => d[truck.Id] == Now), It.IsAny<CancellationToken>()));
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "truck_dispatched"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_TruckInWarehouse_CountsTodaysSales()
    {
        var response = await service.GetByIdAsync(truck.Id);

        Assert.Equal(new DateTime(2026, 10, 15, 4, 0, 0, DateTimeKind.Utc), response.TripFrom);
        Assert.Equal((0, 0m), (response.TripInvoices, response.TripSales));
    }

    [Fact]
    public async Task DispatchAsync_WithoutLoad_IsRejected()
    {
        await Assert.ThrowsAsync<DomainException>(() => service.DispatchAsync(truck.Id));

        Assert.Equal(Truck.StatusInWarehouse, truck.Status);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoadAsync_AddMode_MovesStockFromWarehouseToTruck()
    {
        // Act: 1 caja + 4 und = 40 und.
        await service.LoadAsync(truck.Id, new LoadTruckRequest(beer.Id, TruckLoadMode.Add, 1, 4, null));

        // Assert
        Assert.Equal(320, beer.StockUnits);
        Assert.Equal(40, truck.GetUnits(beer.Id));
        mockTrucks.Verify(r => r.AddLoad(It.Is<TruckLoad>(l => l.TotalUnits == 40 && l.StockBefore == 0 && l.StockAfter == 40)), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "truck_loaded"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadAsync_TargetMode_LoadsOnlyTheDifference()
    {
        // Arrange: el camión tiene 10 und y la meta es 1 caja (36 und).
        truck.AddStock(beer.Id, 10);

        // Act
        await service.LoadAsync(truck.Id, new LoadTruckRequest(beer.Id, TruckLoadMode.Target, 1, 0, "Meta del día"));

        // Assert: carga 26 und del galpón.
        Assert.Equal(36, truck.GetUnits(beer.Id));
        Assert.Equal(334, beer.StockUnits);
        mockTrucks.Verify(r => r.AddLoad(It.Is<TruckLoad>(l => l.TotalUnits == 26 && l.StockBefore == 10)), Times.Once);
    }

    [Fact]
    public async Task LoadAsync_TargetMode_WhenTruckAlreadyHasIt_IsRejectedWithoutChanges()
    {
        // Arrange: el camión tiene 40 und y la meta es 36.
        truck.AddStock(beer.Id, 40);

        // Act
        var error = await Assert.ThrowsAsync<DomainException>(() =>
            service.LoadAsync(truck.Id, new LoadTruckRequest(beer.Id, TruckLoadMode.Target, 1, 0, null)));

        // Assert
        Assert.Equal("El camión ya tiene esa cantidad o más.", error.Message);
        Assert.Equal(360, beer.StockUnits);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockAudit.Verify(a => a.RecordAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoadAsync_MoreThanWarehouseStock_IsRejected()
    {
        // 11 cajas = 396 und, pero el galpón tiene 360.
        await Assert.ThrowsAsync<DomainException>(() =>
            service.LoadAsync(truck.Id, new LoadTruckRequest(beer.Id, TruckLoadMode.Add, 11, 0, null)));

        Assert.Equal(0, truck.GetUnits(beer.Id));
        mockTrucks.Verify(r => r.AddLoad(It.IsAny<TruckLoad>()), Times.Never);
    }

    [Fact]
    public async Task LoadAsync_UnknownTruck_ThrowsKeyNotFound()
    {
        var unknown = Guid.NewGuid();
        mockTrucks.Setup(r => r.GetForUpdateAsync(unknown, It.IsAny<CancellationToken>())).ReturnsAsync((Truck?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.LoadAsync(unknown, new LoadTruckRequest(beer.Id, TruckLoadMode.Add, 1, 0, null)));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicatedPlate_IsRejected()
    {
        mockTrucks.Setup(r => r.PlateExistsAsync("A91AT4L", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new TruckRequest("Otro", "a91at4l")));
        mockTrucks.Verify(r => r.Add(It.IsAny<Truck>()), Times.Never);
    }
}
