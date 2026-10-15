using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Application.Validators;
using Core.Domain.Entities;
using FluentValidation;

namespace UnitTests.Application;

/// <summary>Liquidación ciega de ruta con repositorios simulados (observación 1).</summary>
public sealed class TruckSettlementServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 22, 0, 0, DateTimeKind.Utc);
    private readonly Mock<ITruckRepository> mockTrucks = new();
    private readonly Mock<IProductRepository> mockProducts = new();
    private readonly Mock<IUnitOfWork> mockUnitOfWork = TestDoubles.UnitOfWork();
    private readonly Mock<IAuditService> mockAudit = new();
    private readonly Mock<ICurrentUser> mockCurrentUser = new();
    private readonly Mock<IClock> mockClock = new();
    private readonly TruckSettlementService service;
    private readonly Truck truck = new("NPR", "AB123CD");
    private readonly Product beer = TestDoubles.PolarPilsen(Guid.NewGuid(), stockUnits: 360);

    public TruckSettlementServiceTests()
    {
        service = new TruckSettlementService(mockTrucks.Object, mockProducts.Object, mockUnitOfWork.Object, mockAudit.Object,
            mockCurrentUser.Object, mockClock.Object, new TruckSettlementRequestValidator());
        mockCurrentUser.Setup(u => u.Username).Returns("empleado");
        mockClock.Setup(c => c.UtcNow).Returns(Now);
        mockTrucks.Setup(r => r.GetForUpdateAsync(truck.Id, It.IsAny<CancellationToken>())).ReturnsAsync(truck);
        mockTrucks.Setup(r => r.GetByIdAsync(truck.Id, It.IsAny<CancellationToken>())).ReturnsAsync(truck);
        mockProducts.Setup(r => r.GetByIdAsync(beer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(beer);
        mockProducts.Setup(r => r.GetManyForUpdateAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Product> { [beer.Id] = beer });
        truck.AddStock(beer.Id, 72);
        truck.Dispatch(Now.AddHours(-9));
    }

    [Fact]
    public async Task SettleAsync_ComparesTheBlindCount_WithTheTripCollectionsAndTheTruckStock()
    {
        // Arrange: el sistema registró $300 cobrados en la ruta.
        mockTrucks.Setup(r => r.GetTripSalesAsync(
                It.Is<IReadOnlyDictionary<Guid, DateTime>>(d => d[truck.Id] == Now.AddHours(-9)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, TruckTripSales> { [truck.Id] = new(4, 520m, 300m) });

        // Act: entrega $280 y 1 caja + 30 und (66 und) de las 72 esperadas.
        var sheet = await service.SettleAsync(truck.Id, new TruckSettlementRequest(280m, [new SettlementCountLine(beer.Id, 1, 30)], null));

        // Assert
        Assert.Equal((300m, 280m, -20m), (sheet.ExpectedCash, sheet.CountedCash, sheet.CashDifference));
        var line = Assert.Single(sheet.Lines);
        Assert.Equal((72, 66, -6), (line.ExpectedUnits, line.CountedUnits, line.DifferenceUnits));
        Assert.Equal("2 cajas", line.ExpectedDisplay);
        Assert.False(sheet.IsBalanced);
        Assert.Equal(Truck.StatusSettled, truck.Status);
        mockTrucks.Verify(r => r.AddSettlement(It.IsAny<TruckSettlement>()), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "truck_settled" && e.Amount == -20m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFormAsync_ListsTheProducts_WithoutExpectedQuantities()
    {
        var form = await service.GetFormAsync(truck.Id);

        Assert.Equal(Truck.StatusOnRoute, form.Status);
        var product = Assert.Single(form.Products);
        Assert.Equal((beer.Id, beer.Name, 36), (product.ProductId, product.Name, product.UnitsPerBox));
        Assert.DoesNotContain(typeof(SettlementFormProduct).GetProperties(), p => p.Name.Contains("Units") && p.Name != "UnitsPerBox");
    }

    [Fact]
    public async Task SettleAsync_InvalidCount_FailsValidation_WithoutTouchingTheTruck()
    {
        await Assert.ThrowsAsync<ValidationException>(() => service.SettleAsync(truck.Id,
            new TruckSettlementRequest(-5m, [new SettlementCountLine(beer.Id, 1, 0), new SettlementCountLine(beer.Id, 0, 2)], null)));

        Assert.True(truck.IsOnRoute);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
