using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Application.Validators;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;

namespace UnitTests.Application;

/// <summary>Consignación, averías, consumos internos, compras y ajuste de galpón con repositorios simulados.</summary>
public sealed class OperationServicesTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 15, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IProductRepository> mockProducts = new();
    private readonly Mock<ITruckRepository> mockTrucks = new();
    private readonly Mock<IEmployeeRepository> mockEmployees = new();
    private readonly Mock<IUnitOfWork> mockUnitOfWork = TestDoubles.UnitOfWork();
    private readonly Mock<IAuditService> mockAudit = new();
    private readonly Mock<IClock> mockClock = new();
    private readonly List<Product> catalog = [];
    private readonly Employee employee = new("Juan Rivera", null);

    public OperationServicesTests()
    {
        mockClock.Setup(c => c.UtcNow).Returns(Now);
        mockEmployees.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);
        mockProducts.Setup(r => r.GetManyForUpdateAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => catalog.Where(p => ids.Contains(p.Id)).ToDictionary(p => p.Id));
        mockProducts.Setup(r => r.GetForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => catalog.FirstOrDefault(p => p.Id == id));
    }

    /// <summary>Caja de 24 a $24 y unidad a $1.20.</summary>
    private Product Malta(int stockUnits)
    {
        var product = new Product("MALTIN POLAR 355MLx24UN", "POLAR", "BEB-MAL-0001", Guid.NewGuid(), 24m, 1.20m, 19m, 24, stockUnits, 0, 10_000, false, null);
        catalog.Add(product);
        return product;
    }

    private ConsignmentService Consignments(Mock<IConsignmentRepository> repo) => new(
        repo.Object, mockProducts.Object, mockUnitOfWork.Object, mockAudit.Object, mockClock.Object, Mock.Of<IDocumentPdfGenerator>(),
        new CreateConsignmentRequestValidator(), new ConsignmentReturnRequestValidator());

    private StockWithdrawalService Withdrawals(Mock<IDamagedProductRepository> damages, Mock<IInternalConsumptionRepository> consumptions) => new(
        damages.Object, consumptions.Object, mockProducts.Object, mockTrucks.Object, mockEmployees.Object, mockUnitOfWork.Object,
        mockAudit.Object, mockClock.Object, new CreateDamageRequestValidator(), new CreateInternalConsumptionRequestValidator());

    [Fact]
    public async Task Consignment_DeliverTwoBoxes_ReturnThirtyUnits_ChargesOnlyTheEighteenSold()
    {
        // Arrange
        var malta = Malta(240);
        var repo = new Mock<IConsignmentRepository>();
        ConsignmentEvent? created = null;
        repo.Setup(r => r.Add(It.IsAny<ConsignmentEvent>())).Callback<ConsignmentEvent>(e => created = e);
        var service = Consignments(repo);

        // Act: entrega 2 cajas (48 und) y luego devuelve 30 und.
        var delivered = await service.CreateAsync(new CreateConsignmentRequest("Feria de San Sebastián", "Victor Vera", null, [new LineRequest(malta.Id, 2, 0)]));
        repo.Setup(r => r.GetForUpdateAsync(delivered.Id, It.IsAny<CancellationToken>())).ReturnsAsync(created);
        var afterReturn = await service.RegisterReturnAsync(delivered.Id, new ConsignmentReturnRequest(malta.Id, 0, 30));
        var closed = await service.CloseAsync(delivered.Id);

        // Assert: vendidas 18 und = 0 cajas + 18 × $1.20.
        Assert.Equal(222, malta.StockUnits);
        Assert.Equal(21.60m, afterReturn.TotalSold);
        Assert.True(closed.IsClosed);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "consignment_closed" && e.Amount == 21.60m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consignment_WithoutStock_IsNotCreated()
    {
        var malta = Malta(30);
        var repo = new Mock<IConsignmentRepository>();

        await Assert.ThrowsAsync<DomainException>(() =>
            Consignments(repo).CreateAsync(new CreateConsignmentRequest("Feria", "Victor Vera", null, [new LineRequest(malta.Id, 1, 0), new LineRequest(malta.Id, 0, 10)])));

        Assert.Equal(30, malta.StockUnits);
        repo.Verify(r => r.Add(It.IsAny<ConsignmentEvent>()), Times.Never);
    }

    [Fact]
    public async Task Consignment_ReturnMoreThanPending_IsRejected()
    {
        var malta = Malta(240);
        var repo = new Mock<IConsignmentRepository>();
        ConsignmentEvent? created = null;
        repo.Setup(r => r.Add(It.IsAny<ConsignmentEvent>())).Callback<ConsignmentEvent>(e => created = e);
        var service = Consignments(repo);
        var delivered = await service.CreateAsync(new CreateConsignmentRequest("Feria", "Victor Vera", null, [new LineRequest(malta.Id, 1, 0)]));
        repo.Setup(r => r.GetForUpdateAsync(delivered.Id, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        await Assert.ThrowsAsync<DomainException>(() => service.RegisterReturnAsync(delivered.Id, new ConsignmentReturnRequest(malta.Id, 0, 25)));
        Assert.Equal(216, malta.StockUnits);
    }

    [Fact]
    public async Task Consignment_Indicators_SumOpenAndClosedEvents()
    {
        var malta = Malta(240);
        var open = new ConsignmentEvent("Abierto", "Ana", Now);
        open.Deliver(malta, new Core.Domain.ValueObjects.BoxQuantity(1, 0));
        var closed = new ConsignmentEvent("Cerrado", "Luis", Now);
        closed.Deliver(malta, new Core.Domain.ValueObjects.BoxQuantity(0, 10));
        closed.Close(Now);
        var repo = new Mock<IConsignmentRepository>();
        repo.Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync([open, closed]);

        var indicators = await Consignments(repo).GetIndicatorsAsync();

        Assert.Equal((1, 1), (indicators.OpenEvents, indicators.ClosedEvents));
        Assert.Equal(24m + 12m, indicators.ReceivableTotal);
        Assert.Equal(34, indicators.DeliveredUnits);
    }

    [Fact]
    public async Task Damage_FromTruck_DiscountsTheTruckAndEstimatesTheLoss()
    {
        var malta = Malta(240);
        var truck = new Truck("FVR", "AA200AA", 500);
        truck.AddStock(malta.Id, 48);
        mockTrucks.Setup(r => r.GetForUpdateAsync(truck.Id, It.IsAny<CancellationToken>())).ReturnsAsync(truck);
        var damages = new Mock<IDamagedProductRepository>();

        var damage = await Withdrawals(damages, new()).RegisterDamageAsync(new CreateDamageRequest(
            malta.Id, 1, 2, StockOrigin.Truck, truck.Id, DamageType.Damaged, DamageReason.Broken, DamageAction.Discarded, "Se cayó la gavera"));

        Assert.Equal(22, truck.GetUnits(malta.Id));
        Assert.Equal(240, malta.StockUnits);
        Assert.Equal(26.40m, damage.EstimatedLoss);            // 1 × 24 + 2 × 1.20
        damages.Verify(r => r.Add(It.IsAny<DamagedProduct>()), Times.Once);
    }

    [Fact]
    public async Task Damage_WithInsufficientStock_IsNotRegistered()
    {
        var malta = Malta(10);
        var damages = new Mock<IDamagedProductRepository>();

        var error = await Assert.ThrowsAsync<DomainException>(() => Withdrawals(damages, new()).RegisterDamageAsync(new CreateDamageRequest(
            malta.Id, 1, 0, StockOrigin.Warehouse, null, DamageType.Damaged, DamageReason.Expired, DamageAction.Discarded, null)));

        Assert.StartsWith("Inventario insuficiente", error.Message);
        damages.Verify(r => r.Add(It.IsAny<DamagedProduct>()), Times.Never);
    }

    [Fact]
    public async Task InternalConsumption_RequiresAnActiveEmployee()
    {
        var malta = Malta(240);
        employee.Deactivate();
        var consumptions = new Mock<IInternalConsumptionRepository>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => Withdrawals(new(), consumptions).RegisterConsumptionAsync(
            new CreateInternalConsumptionRequest(malta.Id, 0, 6, StockOrigin.Warehouse, null, employee.Id, InternalConsumptionType.InternalConsumption, null)));

        Assert.Equal(240, malta.StockUnits);
    }

    [Fact]
    public async Task InternalConsumption_DiscountsWarehouseAndEstimatesTheValue()
    {
        var malta = Malta(240);
        var consumptions = new Mock<IInternalConsumptionRepository>();

        var consumption = await Withdrawals(new(), consumptions).RegisterConsumptionAsync(
            new CreateInternalConsumptionRequest(malta.Id, 0, 6, StockOrigin.Warehouse, null, employee.Id, InternalConsumptionType.Gift, null));

        Assert.Equal(234, malta.StockUnits);
        Assert.Equal((7.20m, "Juan Rivera"), (consumption.EstimatedValue, consumption.EmployeeName));
    }

    [Fact]
    public async Task Purchase_AddsStockAndNumbersTheDocument()
    {
        var malta = Malta(0);
        var purchases = new Mock<IPurchaseRepository>();
        var numbers = new Mock<IDocumentNumberGenerator>();
        numbers.Setup(n => n.NextAsync(DocumentSequences.Purchase, 0, 0, 0, It.IsAny<CancellationToken>())).ReturnsAsync(7);
        var service = new PurchaseService(purchases.Object, mockProducts.Object, mockEmployees.Object, numbers.Object,
            mockUnitOfWork.Object, mockAudit.Object, mockClock.Object, new CreatePurchaseRequestValidator());

        var purchase = await service.CreateAsync(new CreatePurchaseRequest(employee.Id, PurchaseDocumentType.Invoice, "F-0012", [new LineRequest(malta.Id, 10, 0)]));

        Assert.Equal("Compra #0007", purchase.Number);
        Assert.Equal((240, 240m), (purchase.TotalUnits, purchase.TotalAmount));
        Assert.Equal(240, malta.StockUnits);
        purchases.Verify(r => r.Add(It.IsAny<Purchase>()), Times.Once);
    }

    [Fact]
    public async Task WarehouseAdjustment_SetsTheAbsoluteStock_AndAuditsBeforeAndAfter()
    {
        var malta = Malta(100);
        mockProducts.Setup(r => r.GetByIdAsync(malta.Id, It.IsAny<CancellationToken>())).ReturnsAsync(malta);
        var service = new InventoryService(mockProducts.Object, Mock.Of<IReportQueries>(), mockUnitOfWork.Object, mockAudit.Object,
            new AdjustWarehouseStockRequestValidator());

        var response = await service.AdjustWarehouseStockAsync(malta.Id, new AdjustWarehouseStockRequest(2, 3, "Conteo físico"));

        Assert.Equal(51, response.StockUnits);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "warehouse_stock_adjusted"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProductLocation_UnknownProduct_ThrowsKeyNotFound()
    {
        var service = new InventoryService(mockProducts.Object, Mock.Of<IReportQueries>(), mockUnitOfWork.Object, mockAudit.Object,
            new AdjustWarehouseStockRequestValidator());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetProductLocationAsync(Guid.NewGuid()));
    }
}
