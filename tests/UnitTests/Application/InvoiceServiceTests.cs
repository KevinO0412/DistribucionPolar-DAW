using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Application.Validators;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;

namespace UnitTests.Application;

/// <summary>
/// Facturación (lógica de negocio §6.1–6.4) con todos los repositorios simulados: saldos con signo,
/// descuento de inventario todo o nada, vacíos por grupo, regalías y anulación.
/// </summary>
public sealed class InvoiceServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 15, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IInvoiceRepository> mockInvoices = new();
    private readonly Mock<IClientRepository> mockClients = new();
    private readonly Mock<IProductRepository> mockProducts = new();
    private readonly Mock<ITruckRepository> mockTrucks = new();
    private readonly Mock<IEmployeeRepository> mockEmployees = new();
    private readonly Mock<IUserRepository> mockUsers = new();
    private readonly Mock<IDocumentNumberGenerator> mockNumbers = new();
    private readonly Mock<IUnitOfWork> mockUnitOfWork = TestDoubles.UnitOfWork();
    private readonly Mock<IAuditService> mockAudit = new();
    private readonly Mock<ICurrentUser> mockCurrentUser = new();
    private readonly Mock<IClock> mockClock = new();
    private readonly InvoiceService service;

    private readonly User employee = new("empleado", "empleado@distribucionpolar.local", "Empleado de Ventas", "HASH", UserRole.Employee, "Carlos Pérez");
    private readonly Client client = new(7, "V-10000004", "Panadería Santa Rosa", null, null, null);
    private readonly Guid beverages = Guid.NewGuid();
    private readonly List<Product> catalog = [];

    public InvoiceServiceTests()
    {
        service = new InvoiceService(
            mockInvoices.Object, mockClients.Object, mockProducts.Object, mockTrucks.Object, mockEmployees.Object, mockUsers.Object,
            mockNumbers.Object, mockUnitOfWork.Object, mockAudit.Object, mockCurrentUser.Object, mockClock.Object,
            Mock.Of<IDocumentPdfGenerator>(), new CreditPolicy(), new CreateSaleRequestValidator(), new CreateGiftRequestValidator(), new CancelInvoiceRequestValidator());

        mockCurrentUser.Setup(u => u.UserId).Returns(employee.Id);
        mockCurrentUser.Setup(u => u.Username).Returns("admin");
        mockUsers.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);
        mockClients.Setup(r => r.GetForUpdateAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        mockClock.Setup(c => c.UtcNow).Returns(Now);
        mockClock.Setup(c => c.Today).Returns(new DateOnly(2026, 10, 15));
        mockNumbers.Setup(n => n.NextAsync(It.IsAny<string>(), 2026, 10, 0, It.IsAny<CancellationToken>())).ReturnsAsync(3);
        mockProducts.Setup(r => r.GetManyForUpdateAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) =>
                catalog.Where(p => ids.Contains(p.Id)).ToDictionary(p => p.Id));
    }

    /// <summary>Caja de 36 a $100 y unidad a $3, del grupo de vacíos 222ML; regalable si se indica.</summary>
    private Product Beer(int stockUnits, bool giftEligible = false)
    {
        var product = new Product("POLAR PILSEN RET 222MLx36UN", "POLAR", "BEB-POL-0001", beverages, 100m, 3m, 80m, 36, stockUnits, 0, 10_000,
            true, "RET_222ML_X36", giftEligible);
        catalog.Add(product);
        return product;
    }

    private Product Flour(int stockUnits)
    {
        var product = TestDoubles.HarinaPan(Guid.NewGuid(), stockUnits);
        catalog.Add(product);
        return product;
    }

    private CreateSaleRequest Sale(Guid productId, int boxes, int loose, decimal payment,
        StockOrigin origin = StockOrigin.Warehouse, Guid? truckId = null, IReadOnlyList<EmptyGroupQuantity>? returned = null, Guid? employeeId = null) =>
        new(client.Id, origin, truckId, employeeId, [new LineRequest(productId, boxes, loose)], payment, "Recibió en $: 40", returned);

    [Fact]
    public async Task CreateSaleAsync_WithPayment_LeavesThePendingAsDebt()
    {
        // Arrange: total 100, abono 40.
        var beer = Beer(360);

        // Act
        var invoice = await service.CreateSaleAsync(Sale(beer.Id, 1, 0, 40m));

        // Assert
        Assert.Equal("Factura Octubre 2026 #3", invoice.Number);
        Assert.Equal((100m, 40m, 60m), (invoice.Total, invoice.Payment, invoice.Pending));
        Assert.Equal(-60m, client.MoneyBalance);
        Assert.Equal(324, beer.StockUnits);
        Assert.Equal("Carlos Pérez", invoice.AttendantName);
        mockInvoices.Verify(r => r.Add(It.IsAny<Invoice>()), Times.Once);
        mockClients.Verify(r => r.AddMovement(It.Is<ClientMovement>(m => m.Type == ClientMovementType.Invoice && m.Amount == 100m)), Times.Once);
        mockClients.Verify(r => r.AddMovement(It.Is<ClientMovement>(m => m.Type == ClientMovementType.Payment && m.Amount == 40m)), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "invoice_created" && e.Amount == 100m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSaleAsync_PaymentOverTotal_LeavesBalanceInFavor()
    {
        var beer = Beer(360);

        var invoice = await service.CreateSaleAsync(Sale(beer.Id, 1, 0, 120m));

        Assert.Equal(-20m, invoice.Pending);
        Assert.Equal(20m, client.MoneyBalance);
    }

    [Fact]
    public async Task CreateSaleAsync_WithoutStockForEveryLine_FailsWithoutChangesOrNumber()
    {
        // Arrange: hay 10 und de harina y se piden 11; la cerveza sí alcanza.
        var beer = Beer(360);
        var flour = Flour(10);
        var request = new CreateSaleRequest(client.Id, StockOrigin.Warehouse, null, null,
            [new LineRequest(beer.Id, 1, 0), new LineRequest(flour.Id, 0, 11)], 0m, null, null);

        // Act
        var error = await Assert.ThrowsAsync<DomainException>(() => service.CreateSaleAsync(request));

        // Assert: ningún cambio y no se consume número de factura.
        Assert.Contains("se solicitan 11 und y hay 10 und", error.Message);
        Assert.Equal((360, 10), (beer.StockUnits, flour.StockUnits));
        Assert.Equal(0m, client.MoneyBalance);
        mockNumbers.Verify(n => n.NextAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        mockInvoices.Verify(r => r.Add(It.IsAny<Invoice>()), Times.Never);
        mockAudit.Verify(a => a.RecordAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSaleAsync_SellThreeBoxesFiveUnits_ReturnTwoBoxes_LeavesOneBoxFiveUnitsOwed()
    {
        var beer = Beer(360);

        var invoice = await service.CreateSaleAsync(Sale(beer.Id, 3, 5, 0m, returned: [new EmptyGroupQuantity("RET_222ML_X36", 2, 0)]));

        Assert.Equal((3, 5, 2, 0), (invoice.GeneratedEmptyBoxes, invoice.GeneratedEmptyUnits, invoice.ReturnedEmptyBoxes, invoice.ReturnedEmptyUnits));
        Assert.Equal((-1, -5), (client.EmptyBoxesBalance, client.EmptyUnitsBalance));
        mockClients.Verify(r => r.AddMovement(It.Is<ClientMovement>(m => m.Type == ClientMovementType.EmptiesGenerated)), Times.Once);
    }

    [Fact]
    public async Task CreateSaleAsync_FromTruck_DiscountsTheTruckNotTheWarehouse()
    {
        var beer = Beer(360);
        var truck = new Truck("FVR", "AA200AA", 500);
        truck.AddStock(beer.Id, 72);
        mockTrucks.Setup(r => r.GetForUpdateAsync(truck.Id, It.IsAny<CancellationToken>())).ReturnsAsync(truck);

        var invoice = await service.CreateSaleAsync(Sale(beer.Id, 1, 0, 0m, StockOrigin.Truck, truck.Id));

        Assert.Equal(("FVR", "AA200AA"), (invoice.TruckName, invoice.TruckPlate));
        Assert.Equal(36, truck.GetUnits(beer.Id));
        Assert.Equal(360, beer.StockUnits);
    }

    [Fact]
    public async Task CreateSaleAsync_FromTruckWithoutTruckId_IsRejected()
    {
        var beer = Beer(360);

        await Assert.ThrowsAnyAsync<Exception>(() => service.CreateSaleAsync(Sale(beer.Id, 1, 0, 0m, StockOrigin.Truck)));
        Assert.Equal(360, beer.StockUnits);
    }

    [Fact]
    public async Task CreateSaleAsync_UserWhoCannotChangeAttendant_CannotPickAnotherEmployee()
    {
        var beer = Beer(360);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateSaleAsync(Sale(beer.Id, 1, 0, 0m, employeeId: Guid.NewGuid())));

        Assert.Equal("Su usuario no tiene permiso para cambiar el vendedor que atiende.", error.Message);
        mockInvoices.Verify(r => r.Add(It.IsAny<Invoice>()), Times.Never);
    }

    [Fact]
    public async Task CreateGiftAsync_HasNoMoneyEffect_ButGeneratesEmpties()
    {
        // Arrange: productos permitidos por $50 en precio.
        var beer = Beer(360, giftEligible: true);
        mockNumbers.Setup(n => n.NextAsync(DocumentSequences.Gift, 2026, 10, 0, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var gift = await service.CreateGiftAsync(new CreateGiftRequest(client.Id, StockOrigin.Warehouse, null, [new LineRequest(beer.Id, 0, 17)]));

        // Assert
        Assert.Equal("Regalía Octubre 2026 #1", gift.Number);
        Assert.Equal((0m, 0m, 0m), (gift.Total, gift.Payment, gift.Pending));
        Assert.Equal(0m, client.MoneyBalance);
        Assert.Equal((0, -17), (client.EmptyBoxesBalance, client.EmptyUnitsBalance));
        Assert.Equal(343, beer.StockUnits);
        mockClients.Verify(r => r.AddMovement(It.Is<ClientMovement>(m => m.Type == ClientMovementType.Payment)), Times.Never);
    }

    [Fact]
    public async Task CreateGiftAsync_WithProductNotAuthorized_IsRejected()
    {
        var flour = Flour(100);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.CreateGiftAsync(new CreateGiftRequest(client.Id, StockOrigin.Warehouse, null, [new LineRequest(flour.Id, 1, 0)])));
        Assert.Equal(100, flour.StockUnits);
    }

    [Fact]
    public async Task CancelAsync_RevertsInventoryMoneyAndEmpties()
    {
        // Arrange: venta de 100 con abono 40 (pendiente 60).
        var beer = Beer(360);
        var sale = await service.CreateSaleAsync(Sale(beer.Id, 1, 0, 40m));
        var invoice = CaptureLastInvoice();
        mockInvoices.Setup(r => r.GetForUpdateAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        // Act
        var cancelled = await service.CancelAsync(sale.Id, new CancelInvoiceRequest("Error de despacho"));

        // Assert
        Assert.True(cancelled.IsCancelled);
        Assert.Equal("admin", cancelled.CancelledBy);
        Assert.Equal(0m, client.MoneyBalance);
        Assert.Equal(360, beer.StockUnits);
        mockClients.Verify(r => r.AddMovement(It.Is<ClientMovement>(m => m.Type == ClientMovementType.InvoiceCancelled && m.Amount == 60m)), Times.Once);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "invoice_cancelled"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_Twice_IsRejected()
    {
        var beer = Beer(360);
        var sale = await service.CreateSaleAsync(Sale(beer.Id, 1, 0, 0m));
        var invoice = CaptureLastInvoice();
        mockInvoices.Setup(r => r.GetForUpdateAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        await service.CancelAsync(sale.Id, new CancelInvoiceRequest("Primera"));

        await Assert.ThrowsAsync<DomainException>(() => service.CancelAsync(sale.Id, new CancelInvoiceRequest("Segunda")));
        Assert.Equal(360, beer.StockUnits);
    }

    [Fact]
    public async Task CancelAsync_TruckSaleWhoseTruckNoLongerExists_FailsWithoutChanges()
    {
        var beer = Beer(360);
        var truck = new Truck("FVR", "AA200AA", 500);
        truck.AddStock(beer.Id, 72);
        mockTrucks.Setup(r => r.GetForUpdateAsync(truck.Id, It.IsAny<CancellationToken>())).ReturnsAsync(truck);
        var sale = await service.CreateSaleAsync(Sale(beer.Id, 1, 0, 0m, StockOrigin.Truck, truck.Id));
        mockInvoices.Setup(r => r.GetForUpdateAsync(sale.Id, It.IsAny<CancellationToken>())).ReturnsAsync(CaptureLastInvoice());
        mockTrucks.Setup(r => r.GetForUpdateAsync(truck.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Truck?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelAsync(sale.Id, new CancelInvoiceRequest("Error")));
        Assert.Equal(-100m, client.MoneyBalance);
    }

    [Fact]
    public async Task CreateSaleAsync_WithIdempotencyKey_StoresTheKey()
    {
        var beer = Beer(360);
        var key = Guid.NewGuid();

        var (invoice, created) = await service.CreateSaleAsync(Sale(beer.Id, 1, 0, 0m), key);

        Assert.True(created);
        Assert.Equal(key, CaptureLastInvoice().IdempotencyKey);
        Assert.Equal(324, beer.StockUnits);
        Assert.Equal("Factura Octubre 2026 #3", invoice.Number);
    }

    [Fact]
    public async Task CreateSaleAsync_RepeatedIdempotencyKey_ReturnsTheExistingInvoiceWithoutSellingAgain()
    {
        // Arrange: la preventa reintenta un pedido que ya llegó.
        var beer = Beer(360);
        var key = Guid.NewGuid();
        await service.CreateSaleAsync(Sale(beer.Id, 1, 0, 0m), key);
        var first = CaptureLastInvoice();
        mockInvoices.Setup(r => r.GetByIdempotencyKeyAsync(key, It.IsAny<CancellationToken>())).ReturnsAsync(first);

        // Act
        var (again, created) = await service.CreateSaleAsync(Sale(beer.Id, 1, 0, 0m), key);

        // Assert: misma factura, sin descontar stock ni cargar saldo dos veces.
        Assert.False(created);
        Assert.Equal(first.Id, again.Id);
        Assert.Equal(324, beer.StockUnits);
        Assert.Equal(-100m, client.MoneyBalance);
        mockInvoices.Verify(r => r.Add(It.IsAny<Invoice>()), Times.Once);
    }

    [Fact]
    public async Task CreateSaleAsync_ClientWithDebtOlderThan30Days_IsBlockedOnCredit()
    {
        // Arrange: debe $50 de una factura de hace 45 días.
        var beer = Beer(360);
        client.AdjustBalances(-50m, 0, 0);
        mockInvoices.Setup(r => r.GetPendingSalesAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Core.Domain.Services.PendingSale(Now.AddDays(-45), 50m)]);

        // Act
        var error = await Assert.ThrowsAsync<DomainException>(() => service.CreateSaleAsync(Sale(beer.Id, 1, 0, 40m)));

        // Assert: no se toca el inventario ni se numera.
        Assert.StartsWith("Crédito bloqueado", error.Message);
        Assert.Equal(360, beer.StockUnits);
        mockNumbers.Verify(n => n.NextAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSaleAsync_ClientWithOverdueDebt_CanStillBuyCash()
    {
        var beer = Beer(360);
        client.AdjustBalances(-50m, 0, 0);
        mockInvoices.Setup(r => r.GetPendingSalesAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Core.Domain.Services.PendingSale(Now.AddDays(-45), 50m)]);

        var invoice = await service.CreateSaleAsync(Sale(beer.Id, 1, 0, 100m));

        Assert.Equal(0m, invoice.Pending);
    }

    [Fact]
    public async Task CreateSaleAsync_OverTheCreditLimit_IsBlocked()
    {
        var beer = Beer(360);
        client.SetCreditLimit(50m);

        var error = await Assert.ThrowsAsync<DomainException>(() => service.CreateSaleAsync(Sale(beer.Id, 1, 0, 40m)));

        Assert.Contains("límite es $50.00", error.Message);
        Assert.Equal(360, beer.StockUnits);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsKeyNotFound()
    {
        mockInvoices.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Invoice?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    private Invoice CaptureLastInvoice() =>
        (Invoice)mockInvoices.Invocations.Last(i => i.Method.Name == nameof(IInvoiceRepository.Add)).Arguments[0];
}
