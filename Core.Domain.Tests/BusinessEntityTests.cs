using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Domain.ValueObjects;

namespace Core.Domain.Tests;

public sealed class ClientTests
{
    [Fact]
    public void NewClient_UsesDefaultsAndZeroBalances()
    {
        var client = TestData.Client();

        Assert.Equal("001", client.FormattedCode);
        Assert.Equal(Client.DefaultAddress, client.Address);
        Assert.Equal(Client.DefaultType, client.Type);
        Assert.Equal(0m, client.MoneyBalance);
        Assert.False(client.IsReceivable);
    }

    [Fact]
    public void InvoicePending_ChargesAccount_NegativeMeansDebt()
    {
        var client = TestData.Client();

        client.ApplyInvoicePending(100m - 40m);

        Assert.Equal(-60m, client.MoneyBalance);
        Assert.True(client.IsReceivable);
    }

    [Fact]
    public void PaymentGreaterThanTotal_LeavesCreditInFavor()
    {
        var client = TestData.Client();

        client.ApplyInvoicePending(100m - 120m);

        Assert.Equal(20m, client.MoneyBalance);
        Assert.False(client.IsReceivable);
    }

    [Fact]
    public void Payment_AddsToBalance()
    {
        var client = TestData.Client();
        client.ApplyInvoicePending(60m);

        client.ApplyPayment(25m);

        Assert.Equal(-35m, client.MoneyBalance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Payment_MustBePositive(decimal amount)
    {
        Assert.Throws<DomainException>(() => TestData.Client().ApplyPayment(amount));
    }

    [Fact]
    public void EmptiesMovement_UpdatesGroupAndTotals()
    {
        var client = TestData.Client();

        client.ApplyEmptiesMovement(EmptyReturnGroups.Ret222MlX36, -1, -5);
        client.ApplyEmptiesMovement(EmptyReturnGroups.Pepsi350MlX24, -2, 0);

        Assert.Equal(-3, client.EmptyBoxesBalance);
        Assert.Equal(-5, client.EmptyUnitsBalance);
        Assert.Equal(2, client.EmptyBalances.Count);
        Assert.Contains(client.EmptyBalances, b => b.GroupKey == EmptyReturnGroups.Ret222MlX36 && b.Boxes == -1 && b.Units == -5);
    }

    [Fact]
    public void EmptiesMovement_RejectsUnknownGroup()
    {
        Assert.Throws<DomainException>(() => TestData.Client().ApplyEmptiesMovement("NO_EXISTE", 1, 0));
    }

    [Fact]
    public void AdjustBalances_SetsAbsoluteValues()
    {
        var client = TestData.Client();

        client.AdjustBalances(-10.50m, -2, 3);

        Assert.Equal(-10.50m, client.MoneyBalance);
        Assert.Equal(-2, client.EmptyBoxesBalance);
        Assert.Equal(3, client.EmptyUnitsBalance);
    }
}

public sealed class TruckTests
{
    [Fact]
    public void AddAndRemoveStock_RemovesLineWhenEmpty()
    {
        var truck = TestData.Truck();
        var productId = Guid.NewGuid();

        truck.AddStock(productId, 10);
        truck.AddStock(productId, 26);
        Assert.Equal(36, truck.GetUnits(productId));

        truck.RemoveStock(productId, 36);
        Assert.Empty(truck.Stock);
    }

    [Fact]
    public void RemoveStock_WhenInsufficient_ThrowsWithoutChanges()
    {
        var truck = TestData.Truck();
        var productId = Guid.NewGuid();
        truck.AddStock(productId, 10);

        Assert.Throws<DomainException>(() => truck.RemoveStock(productId, 11));
        Assert.Equal(10, truck.GetUnits(productId));
    }

    [Fact]
    public void SetStock_ZeroRemovesLine()
    {
        var truck = TestData.Truck();
        var productId = Guid.NewGuid();
        truck.SetStock(productId, 50);

        truck.SetStock(productId, 0);

        Assert.Equal(0, truck.GetUnits(productId));
        Assert.Empty(truck.Stock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(5001)]
    public void Capacity_OutOfRange_IsRejected(int capacity)
    {
        Assert.Throws<DomainException>(() => new Truck("FVR", "AA200AA", capacity));
    }

    [Fact]
    public void Update_ChangesCapacity()
    {
        var truck = TestData.Truck();

        truck.Update("FVR", "aa200aa", 650);

        Assert.Equal((650, "AA200AA"), (truck.CapacityBoxes, truck.Plate));
    }

    [Fact]
    public void NewTruck_WithoutStock_HasZeroOccupancy()
    {
        var truck = new Truck("F-350", "AA500AA", 150);

        Assert.Equal((0m, 0m), (truck.LoadedBoxes, truck.OccupancyPercent));
    }

    [Fact]
    public void Unload_ReturnsEverythingAndEmptiesTruck()
    {
        var truck = TestData.Truck();
        truck.AddStock(Guid.NewGuid(), 10);
        truck.AddStock(Guid.NewGuid(), 5);

        var unloaded = truck.Unload();

        Assert.Equal(15, unloaded.Sum(l => l.Units));
        Assert.Empty(truck.Stock);
    }
}

public sealed class InvoiceTests
{
    private static readonly DateTime Now = new(2026, 10, 2, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Sale_ComputesTotalsAndPending()
    {
        var invoice = Invoice.CreateSale("Factura Octubre 2026 #1", TestData.Client(), StockOrigin.Warehouse, null, null, "Carlos Pérez",
            [new ProductLine(TestData.Pilsen(), new BoxQuantity(2, 5))], 20m, null, new Dictionary<string, BoxQuantity>(), Now);

        Assert.Equal(49.20m, invoice.Total);
        Assert.Equal(29.20m, invoice.Pending);
        Assert.Equal(77, invoice.Items.Single().TotalUnits);
    }

    [Fact]
    public void Sale_GeneratesEmptiesPerGroup_AndRecordsReturns()
    {
        // Vende 3 cajas + 5 und del grupo 222ML y devuelve 2 cajas: pendiente 1 caja + 5 und.
        var invoice = Invoice.CreateSale("Factura Octubre 2026 #2", TestData.Client(), StockOrigin.Warehouse, null, null, "Carlos Pérez",
            [new ProductLine(TestData.Pilsen(), new BoxQuantity(3, 5)), new ProductLine(TestData.Coffee(), new BoxQuantity(1, 0))],
            0m, null, new Dictionary<string, BoxQuantity> { [EmptyReturnGroups.Ret222MlX36] = new(2, 0) }, Now);

        var group = Assert.Single(invoice.EmptyGroups);
        Assert.Equal((3, 5, 2, 0), (group.GeneratedBoxes, group.GeneratedUnits, group.ReturnedBoxes, group.ReturnedUnits));
        Assert.Equal(1, invoice.PendingEmptyBoxes);
        Assert.Equal(5, invoice.PendingEmptyUnits);
    }

    [Fact]
    public void Sale_FromTruck_RequiresTruck()
    {
        Assert.Throws<DomainException>(() => Invoice.CreateSale("F", TestData.Client(), StockOrigin.Truck, null, null, "X",
            [new ProductLine(TestData.Coffee(), new BoxQuantity(1, 0))], 0m, null, new Dictionary<string, BoxQuantity>(), Now));
    }

    [Fact]
    public void Sale_RejectsNegativePaymentAndEmptyLines()
    {
        Assert.Throws<DomainException>(() => Invoice.CreateSale("F", TestData.Client(), StockOrigin.Warehouse, null, null, "X",
            [new ProductLine(TestData.Coffee(), new BoxQuantity(1, 0))], -1m, null, new Dictionary<string, BoxQuantity>(), Now));
        Assert.Throws<DomainException>(() => Invoice.CreateSale("F", TestData.Client(), StockOrigin.Warehouse, null, null, "X",
            [], 0m, null, new Dictionary<string, BoxQuantity>(), Now));
    }

    [Fact]
    public void Gift_HasNoChargeButGeneratesEmpties()
    {
        var gift = Invoice.CreateGift("Regalía Octubre 2026 #1", TestData.Client(), StockOrigin.Warehouse, null, "Carlos Pérez",
            [new ProductLine(TestData.Pilsen(), new BoxQuantity(2, 0))], Now);

        Assert.Equal((0m, 0m, 0m), (gift.Total, gift.Payment, gift.Pending));
        Assert.Equal(0m, gift.Items.Single().Subtotal);
        Assert.Equal(2, gift.GeneratedEmptyBoxes);
        Assert.Equal(Invoice.GiftObservation, gift.PaymentObservation);
    }

    [Fact]
    public void Gift_RejectsProductsNotAuthorized()
    {
        Assert.Throws<DomainException>(() => Invoice.CreateGift("R", TestData.Client(), StockOrigin.Warehouse, null, "X",
            [new ProductLine(TestData.Coffee(), new BoxQuantity(1, 0))], Now));
    }

    [Fact]
    public void Cancel_RequiresReason_AndOnlyOnce()
    {
        var invoice = Invoice.CreateSale("F", TestData.Client(), StockOrigin.Warehouse, null, null, "X",
            [new ProductLine(TestData.Coffee(), new BoxQuantity(1, 0))], 0m, null, new Dictionary<string, BoxQuantity>(), Now);

        Assert.Throws<DomainException>(() => invoice.Cancel(" ", "admin", Now));
        invoice.Cancel("Error de digitación", "admin", Now);
        Assert.True(invoice.IsCancelled);
        Assert.Throws<DomainException>(() => invoice.Cancel("Otra vez", "admin", Now));
    }

    [Fact]
    public void Items_KeepPriceSnapshot()
    {
        var pilsen = TestData.Pilsen();
        var invoice = Invoice.CreateSale("F", TestData.Client(), StockOrigin.Warehouse, null, null, "X",
            [new ProductLine(pilsen, new BoxQuantity(1, 0))], 0m, null, new Dictionary<string, BoxQuantity>(), Now);

        pilsen.UpdateDetails(pilsen.Name, pilsen.Brand, pilsen.CategoryId, 30m, 0.90m, 20m, 36, 180, 7200, true, pilsen.EmptyGroupKey, true);

        Assert.Equal(23m, invoice.Items.Single().PriceBox);
        Assert.Equal(23m, invoice.Total);
    }
}

public sealed class ConsignmentTests
{
    [Fact]
    public void ChargesOnlySoldUnits()
    {
        // Entrega 2 cajas x24 y devuelve 30 und: vendidas 18 und = 0 cajas + 18 × $0.46.
        var pepsi = TestData.Pepsi350();
        var consignment = new ConsignmentEvent("Feria escolar", "María González", DateTime.UtcNow);
        consignment.Deliver(pepsi, new BoxQuantity(2, 0));

        int returned = consignment.RegisterReturn(pepsi.Id, new BoxQuantity(0, 30));

        var item = consignment.Items.Single();
        Assert.Equal(30, returned);
        Assert.Equal(18, item.SoldUnits);
        Assert.Equal(18 * 0.46m, consignment.TotalSold);
    }

    [Fact]
    public void RepeatedProduct_Accumulates()
    {
        var pepsi = TestData.Pepsi350();
        var consignment = new ConsignmentEvent("Evento", "Responsable", DateTime.UtcNow);

        consignment.Deliver(pepsi, new BoxQuantity(1, 0));
        consignment.Deliver(pepsi, new BoxQuantity(0, 6));

        Assert.Equal(30, consignment.Items.Single().DeliveredUnits);
    }

    [Fact]
    public void Return_CannotExceedPending()
    {
        var pepsi = TestData.Pepsi350();
        var consignment = new ConsignmentEvent("Evento", "Responsable", DateTime.UtcNow);
        consignment.Deliver(pepsi, new BoxQuantity(1, 0));

        Assert.Throws<DomainException>(() => consignment.RegisterReturn(pepsi.Id, new BoxQuantity(1, 1)));
    }

    [Fact]
    public void ClosedEvent_RejectsChanges()
    {
        var pepsi = TestData.Pepsi350();
        var consignment = new ConsignmentEvent("Evento", "Responsable", DateTime.UtcNow);
        consignment.Deliver(pepsi, new BoxQuantity(1, 0));
        consignment.Close(DateTime.UtcNow);

        Assert.Throws<DomainException>(() => consignment.RegisterReturn(pepsi.Id, new BoxQuantity(0, 1)));
        Assert.Throws<DomainException>(() => consignment.Close(DateTime.UtcNow));
    }
}

public sealed class WithdrawalAndPurchaseTests
{
    [Fact]
    public void Damage_EstimatedLossIsSubtotal()
    {
        var damage = new DamagedProduct(TestData.Pilsen(), new BoxQuantity(0, 3), StockOrigin.Warehouse, null,
            DamageType.Damaged, DamageReason.Broken, DamageAction.Discarded, null, DateTime.UtcNow);

        Assert.Equal(1.92m, damage.EstimatedLoss);
    }

    [Fact]
    public void Withdrawal_FromTruck_RequiresTruck()
    {
        Assert.Throws<DomainException>(() => new DamagedProduct(TestData.Pilsen(), new BoxQuantity(0, 3), StockOrigin.Truck, null,
            DamageType.Damaged, DamageReason.Broken, DamageAction.Discarded, null, DateTime.UtcNow));
    }

    [Fact]
    public void Purchase_NumbersAndTotals()
    {
        var purchase = new Purchase(1, new Employee("Carlos Pérez", null), PurchaseDocumentType.Invoice, "F-001",
            [new ProductLine(TestData.Pilsen(), new BoxQuantity(10, 0))], DateTime.UtcNow);

        Assert.Equal("Compra #0001", purchase.Number);
        Assert.Equal(360, purchase.TotalUnits);
        Assert.Equal(230m, purchase.TotalAmount);
    }

    [Fact]
    public void Employee_DefaultRoleAndDeactivation()
    {
        var employee = new Employee("Ana Martínez", " ");
        employee.Deactivate();

        Assert.Equal(Employee.DefaultRole, employee.Role);
        Assert.False(employee.IsActive);
    }

    [Fact]
    public void ProductSetStock_IsAbsolute()
    {
        var product = TestData.Pilsen(stockUnits: 100);

        product.SetStock(new BoxQuantity(2, 0));

        Assert.Equal(72, product.StockUnits);
    }
}
