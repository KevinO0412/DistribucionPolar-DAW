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
    private readonly Mock<IInvoiceRepository> mockInvoices = new();
    private readonly Mock<IUnitOfWork> mockUnitOfWork = TestDoubles.UnitOfWork();
    private readonly Mock<IAuditService> mockAudit = new();
    private readonly Mock<ICurrentUser> mockCurrentUser = new();
    private readonly TruckService service;
    private readonly Truck truck = new("815-CARGO", "A91AT4L");
    private readonly Product beer = TestDoubles.PolarPilsen(Guid.NewGuid(), stockUnits: 360);

    public TruckServiceTests()
    {
        mockCurrentUser.Setup(u => u.Username).Returns("empleado");
        service = new TruckService(
            mockTrucks.Object, mockProducts.Object, mockInvoices.Object, mockUnitOfWork.Object, mockAudit.Object, mockCurrentUser.Object,
            Mock.Of<IClock>(), new TruckRequestValidator(), new LoadTruckRequestValidator(),
            new TransferFromTruckRequestValidator(), new AdjustStockRequestValidator(), new SettleRouteRequestValidator());

        mockTrucks.Setup(r => r.GetForUpdateAsync(truck.Id, It.IsAny<CancellationToken>())).ReturnsAsync(truck);
        mockTrucks.Setup(r => r.GetByIdAsync(truck.Id, It.IsAny<CancellationToken>())).ReturnsAsync(truck);
        mockProducts.Setup(r => r.GetForUpdateAsync(beer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(beer);
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
    public async Task LoadAsync_LeavesTheTruckLoading()
    {
        await service.LoadAsync(truck.Id, new LoadTruckRequest(beer.Id, TruckLoadMode.Add, 1, 0, null));

        Assert.Equal(Truck.StatusLoading, truck.Status);
    }

    [Fact]
    public async Task DispatchAsync_PutsTheTruckOnRoute()
    {
        truck.AddStock(beer.Id, 36);

        var response = await service.DispatchAsync(truck.Id);

        Assert.Equal(Truck.StatusOnRoute, response.Status);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "truck_dispatched"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SettleAsync_BlindSettlement_ReturnsCountedGoodsAndReportsDifferences()
    {
        // Arrange: sale con 72 und; durante la ruta factura $230 y cobra $40 de abonos.
        truck.AddStock(beer.Id, 72);
        truck.Dispatch(new DateTime(2026, 10, 15, 8, 0, 0, DateTimeKind.Utc));
        mockInvoices.Setup(r => r.GetTruckRouteSalesAsync(truck.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((3, 230m, 40m));
        mockProducts.Setup(r => r.GetManyForUpdateAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Product> { [beer.Id] = beer });
        RouteSettlement? saved = null;
        mockTrucks.Setup(r => r.AddSettlement(It.IsAny<RouteSettlement>())).Callback<RouteSettlement>(s => saved = s);

        // Act: el chofer declara $35 y 1 caja + 24 und (60 und), sin ver lo esperado.
        var planilla = await service.SettleAsync(truck.Id, new SettleRouteRequest(35m, [new LineRequest(beer.Id, 1, 24)], null));

        // Assert
        Assert.Equal((40m, 35m, -5m), (planilla.ExpectedCash, planilla.CountedCash, planilla.CashDifference));
        Assert.Equal((72, 60, -12), (planilla.Lines[0].ExpectedUnits, planilla.Lines[0].CountedUnits, planilla.Lines[0].DifferenceUnits));
        Assert.Equal(420, beer.StockUnits);                     // 360 + 60 contadas
        Assert.Equal(Truck.StatusSettled, truck.Status);
        Assert.NotNull(saved);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SettleAsync_TruckNotOnRoute_IsRejected()
    {
        await Assert.ThrowsAsync<DomainException>(() => service.SettleAsync(truck.Id, new SettleRouteRequest(0m, [], null)));
        mockTrucks.Verify(r => r.AddSettlement(It.IsAny<RouteSettlement>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicatedPlate_IsRejected()
    {
        mockTrucks.Setup(r => r.PlateExistsAsync("A91AT4L", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new TruckRequest("Otro", "a91at4l")));
        mockTrucks.Verify(r => r.Add(It.IsAny<Truck>()), Times.Never);
    }
}
