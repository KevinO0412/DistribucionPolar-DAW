using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Core.Application.DTOs;
using Core.Domain.Enums;

namespace IntegrationTests;

/// <summary>Observaciones del profesor (docs/09-OBSERVACIONES-PROFESOR.md) de punta a punta contra PostgreSQL real.</summary>
[Collection(ApiCollection.Name)]
public sealed class ObservationTests(ApiFixture api)
{
    private static async Task<ProductResponse> NewProductAsync(HttpClient admin, decimal priceBox, decimal priceUnit, int unitsPerBox, int boxes)
    {
        var request = new CreateProductRequest($"PRODUCTO PRUEBA {Guid.NewGuid():N}"[..40], "Prueba", ApiFixture.BeveragesCategoryId,
            priceBox, priceUnit, priceBox * 0.8m, unitsPerBox, boxes, 0, 0, 100_000, false, null, false);
        return await admin.PostAsync<ProductResponse>("/api/products", request, HttpStatusCode.Created);
    }

    private static Task<ClientResponse> NewClientAsync(HttpClient http) =>
        http.PostAsync<ClientResponse>("/api/clients",
            new CreateClientRequest($"V-{Random.Shared.Next(10_000_000, 99_999_999)}", "Cliente de prueba", null, null), HttpStatusCode.Created);

    private static CreateSaleRequest Sale(Guid clientId, Guid productId, int boxes, decimal payment) =>
        new(clientId, StockOrigin.Warehouse, null, null, [new LineRequest(productId, boxes, 0)], payment, null, null);

    private static async Task<TruckResponse> LoadedTruckAsync(HttpClient admin, Guid productId, int boxes)
    {
        string plate = $"T{Random.Shared.Next(100_000, 999_999)}";
        var truck = await admin.PostAsync<TruckResponse>("/api/trucks", new TruckRequest($"Prueba {plate}", plate, 200), HttpStatusCode.Created);
        return await admin.PostAsync<TruckResponse>($"/api/trucks/{truck.Id}/load", new LoadTruckRequest(productId, TruckLoadMode.Add, boxes, 0, null));
    }

    private static Task<HttpResponseMessage> PostSaleAsync(HttpClient http, CreateSaleRequest sale, string? idempotencyKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/invoices/sales")
        {
            Content = JsonContent.Create(sale, options: ApiFixture.Json),
        };
        if (idempotencyKey is not null)
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        }

        return http.SendAsync(request);
    }

    [Fact]
    public async Task OfflineOrderResent_WithSameIdempotencyKey_IsInvoicedOnlyOnce()
    {
        var employee = await api.EmployeeAsync();
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 100m, 12m, 10, 10);
        var client = await NewClientAsync(admin);
        var sale = Sale(client.Id, product.Id, 2, 50m);
        string key = Guid.NewGuid().ToString();

        // El teléfono envía el pedido, pierde la respuesta y lo reenvía al reconectarse.
        var first = await PostSaleAsync(employee, sale, key);
        var retry = await PostSaleAsync(employee, sale, key);
        var conflict = await PostSaleAsync(employee, sale with { Payment = 10m }, key);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Equal("true", retry.Headers.GetValues("Idempotency-Replayed").Single());
        var a = JsonSerializer.Deserialize<InvoiceResponse>(await first.Content.ReadAsStringAsync(), ApiFixture.Json)!;
        var b = JsonSerializer.Deserialize<InvoiceResponse>(await retry.Content.ReadAsStringAsync(), ApiFixture.Json)!;
        Assert.Equal(a.Id, b.Id);
        Assert.Equal((HttpStatusCode)422, conflict.StatusCode);
        Assert.Equal("application/problem+json", conflict.Content.Headers.ContentType?.MediaType);

        // Una sola venta: stock y saldo descontados una vez.
        var after = await admin.GetAsync<ProductResponse>($"/api/products/{product.Id}");
        var balance = await admin.GetAsync<ClientResponse>($"/api/clients/{client.Id}");
        Assert.Equal(80, after.StockUnits);
        Assert.Equal(-150m, balance.MoneyBalance);
    }

    [Fact]
    public async Task CreditLimit_BlocksCreditSales_ButAllowsCash_AndOnlyAdminSetsIt()
    {
        var admin = await api.AdminAsync();
        var employee = await api.EmployeeAsync();
        var product = await NewProductAsync(admin, 100m, 12m, 10, 10);
        var client = await NewClientAsync(admin);

        var forbidden = await employee.SendRawAsync(HttpMethod.Put, $"/api/clients/{client.Id}/credit", new CreditTermsRequest(150m, 30));
        var terms = await admin.SendAsync<CreditStatusResponse>(HttpMethod.Put, $"/api/clients/{client.Id}/credit", new CreditTermsRequest(150m, 30));

        // Debe 100 (dentro del límite); la siguiente venta a crédito dejaría 200 > 150.
        await employee.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, product.Id, 1, 0m), HttpStatusCode.Created);
        var blocked = await employee.SendRawAsync(HttpMethod.Post, "/api/invoices/sales", Sale(client.Id, product.Id, 1, 0m));
        var cash = await employee.SendRawAsync(HttpMethod.Post, "/api/invoices/sales", Sale(client.Id, product.Id, 1, 100m));
        var credit = await employee.GetAsync<CreditStatusResponse>($"/api/clients/{client.Id}/credit");

        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Equal(150m, terms.Available);
        Assert.Equal(HttpStatusCode.BadRequest, blocked.StatusCode);
        Assert.Contains("supera el límite de crédito", await blocked.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Created, cash.StatusCode);
        Assert.Equal((100m, 50m, 100m), (credit.Debt, credit.Available, credit.Days0To30));
        Assert.False(credit.IsBlocked);
    }

    [Fact]
    public async Task Dashboard_IncludesTheDebtAgingOfTheClients()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 100m, 12m, 10, 10);
        var client = await NewClientAsync(admin);
        await admin.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, product.Id, 1, 30m), HttpStatusCode.Created);

        var dashboard = await admin.GetAsync<DashboardResponse>("/api/reports/dashboard");

        var receivables = dashboard.Receivables;
        Assert.True(receivables.TotalDebt >= 70m && receivables.Debtors >= 1);
        Assert.Equal(receivables.TotalDebt, receivables.Days0To30 + receivables.Days31To60 + receivables.Days61To90 + receivables.Over90);
        Assert.True(receivables.TopDebtors.Count <= 10);
        Assert.Equal(receivables.TopDebtors.OrderByDescending(r => r.Debt).Select(r => r.ClientId), receivables.TopDebtors.Select(r => r.ClientId));
    }

    [Fact]
    public async Task Truck_GoesLoadingThenOnRoute_AndShowsTheTripSales()
    {
        var admin = await api.AdminAsync();
        var employee = await api.EmployeeAsync();
        var product = await NewProductAsync(admin, 100m, 12m, 10, 10);
        var client = await NewClientAsync(admin);
        var loading = await LoadedTruckAsync(admin, product.Id, 5);

        var dispatched = await employee.PostAsync<TruckResponse>($"/api/trucks/{loading.Id}/dispatch", null);
        var again = await employee.SendRawAsync(HttpMethod.Post, $"/api/trucks/{loading.Id}/dispatch");
        await employee.PostAsync<InvoiceResponse>("/api/invoices/sales",
            new CreateSaleRequest(client.Id, StockOrigin.Truck, loading.Id, null, [new LineRequest(product.Id, 2, 0)], 150m, null, null),
            HttpStatusCode.Created);
        var onRoute = await employee.GetAsync<TruckResponse>($"/api/trucks/{loading.Id}");

        Assert.Equal("Cargando", loading.Status);
        Assert.Equal("En ruta", dispatched.Status);
        Assert.NotNull(dispatched.DispatchedAt);
        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
        Assert.Equal((1, 200m, 150m), (onRoute.TripInvoices, onRoute.TripSales, onRoute.TripCollected));
        Assert.Equal(30, onRoute.TotalUnits);
    }

    [Fact]
    public async Task BlindSettlement_ComparesTheCount_WithTheRoute_AndClosesIt()
    {
        var admin = await api.AdminAsync();
        var employee = await api.EmployeeAsync();
        var product = await NewProductAsync(admin, 100m, 12m, 10, 10);
        var client = await NewClientAsync(admin);
        var truck = await LoadedTruckAsync(admin, product.Id, 5);
        await employee.PostAsync<TruckResponse>($"/api/trucks/{truck.Id}/dispatch", null);
        await employee.PostAsync<InvoiceResponse>("/api/invoices/sales",
            new CreateSaleRequest(client.Id, StockOrigin.Truck, truck.Id, null, [new LineRequest(product.Id, 2, 0)], 150m, null, null),
            HttpStatusCode.Created);

        // El formulario no revela cantidades ni cobros.
        string form = await (await employee.SendRawAsync(HttpMethod.Get, $"/api/trucks/{truck.Id}/settlement-form")).Content.ReadAsStringAsync();
        // Devuelve $140 (esperado 150) y 2 cajas + 5 und (esperado 3 cajas = 30 und).
        var sheet = await employee.PostAsync<TruckSettlementResponse>($"/api/trucks/{truck.Id}/settlements",
            new TruckSettlementRequest(140m, [new SettlementCountLine(product.Id, 2, 5)], "Faltan 5 und"), HttpStatusCode.Created);
        var settled = await employee.GetAsync<TruckResponse>($"/api/trucks/{truck.Id}");
        var again = await employee.SendRawAsync(HttpMethod.Post, $"/api/trucks/{truck.Id}/settlements", new TruckSettlementRequest(0m, [], null));
        var stored = await admin.GetAsync<TruckSettlementResponse>($"/api/trucks/settlements/{sheet.Id}");

        Assert.Contains(product.Name, form);
        Assert.DoesNotContain("stockUnits", form);
        Assert.Equal((150m, 140m, -10m), (sheet.ExpectedCash, sheet.CountedCash, sheet.CashDifference));
        var line = Assert.Single(sheet.Lines);
        Assert.Equal((30, 25, -5, -60m), (line.ExpectedUnits, line.CountedUnits, line.DifferenceUnits, line.DifferenceValue));
        Assert.Equal("Liquidado", settled.Status);
        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
        Assert.Equal(sheet.Id, stored.Id);
        Assert.False(stored.IsBalanced);
    }
}
