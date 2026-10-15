using System.Globalization;
using System.Net;
using Core.Application.DTOs;
using Core.Domain.Enums;

namespace IntegrationTests;

/// <summary>Casos de referencia de la lógica de negocio, ejecutados de punta a punta contra PostgreSQL real.</summary>
[Collection(ApiCollection.Name)]
public sealed class BusinessScenarioTests(ApiFixture api)
{
    private static readonly string[] Months = CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.MonthNames;

    private async Task<ProductResponse> NewProductAsync(HttpClient admin, decimal priceBox, decimal priceUnit, int unitsPerBox,
        int boxes, int looseUnits, string? emptyGroup = null, bool giftEligible = false)
    {
        var request = new CreateProductRequest($"PRODUCTO PRUEBA {Guid.NewGuid():N}"[..40], "Prueba", ApiFixture.BeveragesCategoryId,
            priceBox, priceUnit, priceBox * 0.8m, unitsPerBox, boxes, looseUnits, 0, 100_000, emptyGroup is not null, emptyGroup, giftEligible);
        return await admin.PostAsync<ProductResponse>("/api/products", request, HttpStatusCode.Created);
    }

    private static Task<ClientResponse> NewClientAsync(HttpClient http) =>
        http.PostAsync<ClientResponse>("/api/clients",
            new CreateClientRequest($"V-{Random.Shared.Next(10_000_000, 99_999_999)}", "Cliente de prueba", null, null), HttpStatusCode.Created);

    private static CreateSaleRequest Sale(Guid clientId, Guid productId, int boxes, int loose, decimal payment,
        StockOrigin origin = StockOrigin.Warehouse, Guid? truckId = null, IReadOnlyList<EmptyGroupQuantity>? returned = null) =>
        new(clientId, origin, truckId, null, [new LineRequest(productId, boxes, loose)], payment, null, returned);

    [Fact]
    public async Task SaleWithPayment_LeavesPendingAsDebt_AndPaymentOverTotalLeavesCredit()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 100m, 12m, 10, 10, 0);

        var debtor = await NewClientAsync(admin);
        var invoice = await admin.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(debtor.Id, product.Id, 1, 0, 40m), HttpStatusCode.Created);
        var afterDebt = await admin.GetAsync<ClientResponse>($"/api/clients/{debtor.Id}");

        var creditor = await NewClientAsync(admin);
        await admin.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(creditor.Id, product.Id, 1, 0, 120m), HttpStatusCode.Created);
        var afterCredit = await admin.GetAsync<ClientResponse>($"/api/clients/{creditor.Id}");

        Assert.Equal(60m, invoice.Pending);
        Assert.Equal(-60m, afterDebt.MoneyBalance);
        Assert.Equal("Debe: $60.00", afterDebt.MoneyStatus);
        Assert.Equal(20m, afterCredit.MoneyBalance);
        Assert.Equal("Saldo a favor: $20.00", afterCredit.MoneyStatus);
    }

    [Fact]
    public async Task Payment_ReducesDebt()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 100m, 12m, 10, 10, 0);
        var client = await NewClientAsync(admin);
        await admin.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, product.Id, 1, 0, 40m), HttpStatusCode.Created);

        var after = await admin.PostAsync<ClientResponse>($"/api/clients/{client.Id}/payments", new ClientPaymentRequest(25m, "Efectivo"));

        Assert.Equal(-35m, after.MoneyBalance);
    }

    [Fact]
    public async Task OutOfStock_FailsWithoutAnyChange_AndDoesNotConsumeInvoiceNumber()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 10m, 1m, 10, 1, 0);   // 10 und en galpón
        var other = await NewProductAsync(admin, 10m, 1m, 10, 5, 0);
        var client = await NewClientAsync(admin);

        var first = await admin.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, other.Id, 1, 0, 0m), HttpStatusCode.Created);
        var failure = await admin.SendRawAsync(HttpMethod.Post, "/api/invoices/sales",
            new CreateSaleRequest(client.Id, StockOrigin.Warehouse, null, null,
                [new LineRequest(other.Id, 1, 0), new LineRequest(product.Id, 0, 11)], 0m, null, null));
        var second = await admin.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, other.Id, 1, 0, 0m), HttpStatusCode.Created);

        Assert.Equal(HttpStatusCode.BadRequest, failure.StatusCode);
        Assert.Contains("Inventario insuficiente", await failure.Content.ReadAsStringAsync());
        Assert.Equal(10, (await admin.GetAsync<ProductResponse>($"/api/products/{product.Id}")).StockUnits);
        Assert.Equal(30, (await admin.GetAsync<ProductResponse>($"/api/products/{other.Id}")).StockUnits);
        Assert.Equal(-20m, (await admin.GetAsync<ClientResponse>($"/api/clients/{client.Id}")).MoneyBalance);
        Assert.Equal(SequenceOf(first.Number) + 1, SequenceOf(second.Number));
    }

    [Fact]
    public async Task InvoiceNumber_IsMonthlyWithYear()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 10m, 1m, 10, 5, 0);
        var client = await NewClientAsync(admin);

        var invoice = await admin.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, product.Id, 1, 0, 0m), HttpStatusCode.Created);

        var local = TimeZoneInfo.ConvertTimeFromUtc(invoice.IssuedAt, TimeZoneInfo.FindSystemTimeZoneById("America/Caracas"));
        string month = char.ToUpper(Months[local.Month - 1][0]) + Months[local.Month - 1][1..];
        Assert.Matches($"^Factura {month} {local.Year} #\\d+$", invoice.Number);
    }

    [Fact]
    public async Task Empties_SellThreeBoxesFiveUnits_ReturnTwoBoxes_LeavesOneBoxFiveUnitsOwed()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 23m, 0.64m, 36, 10, 0, "RET_222ML_X36");
        var client = await NewClientAsync(admin);

        await admin.PostAsync<InvoiceResponse>("/api/invoices/sales",
            Sale(client.Id, product.Id, 3, 5, 0m, returned: [new EmptyGroupQuantity("RET_222ML_X36", 2, 0)]), HttpStatusCode.Created);
        var after = await admin.GetAsync<ClientResponse>($"/api/clients/{client.Id}");

        Assert.Equal(-1, after.EmptyBoxesBalance);
        Assert.Equal(-5, after.EmptyUnitsBalance);
        var group = Assert.Single(after.EmptyBalances);
        Assert.Equal(("RET_222ML_X36", -1, -5), (group.GroupKey, group.Boxes, group.Units));
    }

    [Fact]
    public async Task Gift_HasNoMoneyEffect_ButGeneratesEmpties()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 25m, 0.75m, 36, 10, 0, "RET_222ML_X36", giftEligible: true);
        var client = await NewClientAsync(admin);

        var gift = await admin.PostAsync<InvoiceResponse>("/api/invoices/gifts",
            new CreateGiftRequest(client.Id, StockOrigin.Warehouse, null, [new LineRequest(product.Id, 2, 0)]), HttpStatusCode.Created);
        var after = await admin.GetAsync<ClientResponse>($"/api/clients/{client.Id}");

        Assert.Equal(InvoiceType.Gift, gift.Type);
        Assert.Equal((0m, 0m, 0m), (gift.Total, gift.Payment, gift.Pending));
        Assert.Equal(0m, after.MoneyBalance);
        Assert.Equal(-2, after.EmptyBoxesBalance);
    }

    [Fact]
    public async Task Dashboard_ReflectsTruckSaleOccupancyAndRotation_AndIsAdminOnly()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 24m, 1m, 24, 20, 0);
        var truck = await admin.PostAsync<TruckResponse>("/api/trucks", new TruckRequest("Dashboard", $"D{Random.Shared.Next(100000, 999999)}", 40), HttpStatusCode.Created);
        var loaded = await admin.PostAsync<TruckResponse>($"/api/trucks/{truck.Id}/load", new LoadTruckRequest(product.Id, TruckLoadMode.Add, 10, 0, null));
        var client = await NewClientAsync(admin);
        var before = await admin.GetAsync<DashboardResponse>($"/api/reports/dashboard?categoryId={ApiFixture.BeveragesCategoryId}");

        await admin.PostAsync<InvoiceResponse>("/api/invoices/sales",
            Sale(client.Id, product.Id, 2, 0, 48m, StockOrigin.Truck, truck.Id), HttpStatusCode.Created);
        var after = await admin.GetAsync<DashboardResponse>($"/api/reports/dashboard?categoryId={ApiFixture.BeveragesCategoryId}");
        var employee = await (await api.EmployeeAsync()).SendRawAsync(HttpMethod.Get, "/api/reports/dashboard", null);
        var invalid = await admin.SendRawAsync(HttpMethod.Get, "/api/reports/dashboard?from=2026-10-10&to=2026-10-01", null);

        Assert.Equal(25.0m, loaded.OccupancyPercent);                               // 10 de 40 cajas
        Assert.Equal(48m, after.Sales.SalesTotal - before.Sales.SalesTotal);
        Assert.Equal(1, after.Sales.InvoicesCount - before.Sales.InvoicesCount);
        var truckKpi = after.Fleet.Trucks.Single(t => t.TruckId == truck.Id);
        Assert.Equal((20.0m, 48m, 1), (truckKpi.OccupancyPercent, truckKpi.SalesTotal, truckKpi.Invoices));
        Assert.Contains(after.TopProducts, p => p.ProductId == product.Id && p.UnitsSold == 48 && p.StockUnits == 432);
        Assert.All(after.SalesByCategory, c => Assert.Equal(ApiFixture.BeveragesCategoryId, c.CategoryId));
        Assert.Equal(HttpStatusCode.Forbidden, employee.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Cancellation_RevertsInventoryMoneyAndEmpties_AndIsExcludedFromReports()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 100m, 3m, 36, 10, 0, "RET_222ML_X36");
        var truck = await admin.PostAsync<TruckResponse>("/api/trucks", new TruckRequest("Prueba", $"T{Random.Shared.Next(100000, 999999)}"), HttpStatusCode.Created);
        await admin.PostAsync<TruckResponse>($"/api/trucks/{truck.Id}/load", new LoadTruckRequest(product.Id, TruckLoadMode.Add, 5, 0, null));
        var client = await NewClientAsync(admin);
        var reportBefore = await admin.GetAsync<DailyReportResponse>("/api/reports/daily");

        var invoice = await admin.PostAsync<InvoiceResponse>("/api/invoices/sales",
            Sale(client.Id, product.Id, 1, 5, 55m, StockOrigin.Truck, truck.Id, [new EmptyGroupQuantity("RET_222ML_X36", 0, 0)]), HttpStatusCode.Created);
        var owed = await admin.GetAsync<ClientResponse>($"/api/clients/{client.Id}");

        var cancelled = await admin.PostAsync<InvoiceResponse>($"/api/invoices/{invoice.Id}/cancel", new CancelInvoiceRequest("Error de despacho"));
        var restored = await admin.GetAsync<ClientResponse>($"/api/clients/{client.Id}");
        var truckAfter = await admin.GetAsync<TruckResponse>($"/api/trucks/{truck.Id}");
        var reportAfter = await admin.GetAsync<DailyReportResponse>("/api/reports/daily");
        var again = await admin.SendRawAsync(HttpMethod.Post, $"/api/invoices/{invoice.Id}/cancel", new CancelInvoiceRequest("Otra vez"));

        Assert.Equal(60m, invoice.Pending);                                   // 100 + 5 × 3 = 115 − 55
        Assert.Equal((-60m, -1, -5), (owed.MoneyBalance, owed.EmptyBoxesBalance, owed.EmptyUnitsBalance));
        Assert.True(cancelled.IsCancelled);
        Assert.Equal((0m, 0, 0), (restored.MoneyBalance, restored.EmptyBoxesBalance, restored.EmptyUnitsBalance));
        Assert.Equal(180, truckAfter.Stock.Single().StockUnits);
        Assert.Equal(reportBefore.SalesCount, reportAfter.SalesCount);
        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
    }

    [Fact]
    public async Task TruckLoad_TargetMode_LoadsTheDifference_OrRejectsWhenAlreadyAtTarget()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 10m, 1m, 36, 10, 0);
        var truck = await admin.PostAsync<TruckResponse>("/api/trucks", new TruckRequest("Meta", $"M{Random.Shared.Next(100000, 999999)}"), HttpStatusCode.Created);
        await admin.PostAsync<TruckResponse>($"/api/trucks/{truck.Id}/load", new LoadTruckRequest(product.Id, TruckLoadMode.Add, 0, 10, null));

        var loaded = await admin.PostAsync<TruckResponse>($"/api/trucks/{truck.Id}/load", new LoadTruckRequest(product.Id, TruckLoadMode.Target, 1, 0, null));
        await admin.PostAsync<TruckResponse>($"/api/trucks/{truck.Id}/load", new LoadTruckRequest(product.Id, TruckLoadMode.Add, 0, 4, null));
        var rejected = await admin.SendRawAsync(HttpMethod.Post, $"/api/trucks/{truck.Id}/load", new LoadTruckRequest(product.Id, TruckLoadMode.Target, 1, 0, null));
        var warehouse = await admin.GetAsync<ProductResponse>($"/api/products/{product.Id}");

        Assert.Equal(36, loaded.Stock.Single().StockUnits);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Contains("ya tiene esa cantidad o más", await rejected.Content.ReadAsStringAsync());
        Assert.Equal(360 - 40, warehouse.StockUnits);
    }

    [Fact]
    public async Task Consignment_ChargesOnlySoldUnits()
    {
        var employee = await api.EmployeeAsync();
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 11m, 0.46m, 24, 10, 0);

        var created = await employee.PostAsync<ConsignmentResponse>("/api/consignments",
            new CreateConsignmentRequest("Feria", "Responsable", null, [new LineRequest(product.Id, 2, 0)]), HttpStatusCode.Created);
        var returned = await employee.PostAsync<ConsignmentResponse>($"/api/consignments/{created.Id}/returns", new ConsignmentReturnRequest(product.Id, 0, 30));
        var closed = await employee.PostAsync<ConsignmentResponse>($"/api/consignments/{created.Id}/close", null);
        var warehouse = await admin.GetAsync<ProductResponse>($"/api/products/{product.Id}");

        var item = returned.Items.Single();
        Assert.Equal(18, item.SoldUnits);
        Assert.Equal(18 * 0.46m, closed.TotalSold);
        Assert.Equal(240 - 48 + 30, warehouse.StockUnits);
    }

    [Fact]
    public async Task ConcurrentSales_OfTheLastUnits_NeverOversell()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 10m, 1m, 10, 1, 0);   // solo 10 und
        var clients = new List<ClientResponse>();
        for (int i = 0; i < 5; i++)
        {
            clients.Add(await NewClientAsync(admin));
        }

        var responses = await Task.WhenAll(clients.Select(async c =>
        {
            var http = await api.AdminAsync();
            return await http.SendRawAsync(HttpMethod.Post, "/api/invoices/sales", Sale(c.Id, product.Id, 1, 0, 0m));
        }));

        var statuses = responses.Select(r => r.StatusCode).ToList();
        Assert.Equal(1, statuses.Count(s => s == HttpStatusCode.Created));
        Assert.All(statuses.Where(s => s != HttpStatusCode.Created), s => Assert.Contains(s, new[] { HttpStatusCode.BadRequest, HttpStatusCode.Conflict }));
        Assert.Equal(0, (await admin.GetAsync<ProductResponse>($"/api/products/{product.Id}")).StockUnits);
    }

    [Fact]
    public async Task Permissions_EmployeeOperates_ButCannotAdminister()
    {
        var employee = await api.EmployeeAsync();
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 10m, 1m, 10, 5, 0);
        var client = await NewClientAsync(employee);

        var sale = await employee.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, product.Id, 1, 0, 0m), HttpStatusCode.Created);

        Assert.Equal(HttpStatusCode.Forbidden, (await employee.SendRawAsync(HttpMethod.Post, $"/api/invoices/{sale.Id}/cancel", new CancelInvoiceRequest("x"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await employee.SendRawAsync(HttpMethod.Post, "/api/purchases",
            new CreatePurchaseRequest(ApiFixture.SeedEmployeeId, PurchaseDocumentType.Invoice, null, [new LineRequest(product.Id, 1, 0)]))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await employee.SendRawAsync(HttpMethod.Get, "/api/reports/daily")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await employee.SendRawAsync(HttpMethod.Delete, $"/api/clients/{client.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await employee.SendRawAsync(HttpMethod.Post, "/api/invoices/sales",
            new CreateSaleRequest(client.Id, StockOrigin.Warehouse, null, ApiFixture.SeedEmployeeId, [new LineRequest(product.Id, 1, 0)], 0m, null, null))).StatusCode);
    }

    [Fact]
    public async Task Audit_RecordsBusinessOperations()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 10m, 1m, 10, 5, 0);
        var client = await NewClientAsync(admin);
        var sale = await admin.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, product.Id, 1, 0, 0m), HttpStatusCode.Created);

        var log = await admin.GetAsync<PagedResponse<AuditLogResponse>>("/api/reports/audit?module=invoices&pageSize=200");

        var entry = Assert.Single(log.Items, a => a.EntityId == sale.Id.ToString());
        Assert.Equal("invoice_created", entry.Action);
        Assert.Equal("admin", entry.Username);
        Assert.Contains("\"payment\"", entry.MetadataJson);
    }

    private static int SequenceOf(string number) => int.Parse(number[(number.LastIndexOf('#') + 1)..], CultureInfo.InvariantCulture);
}
