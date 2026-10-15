using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace IntegrationTests;

/// <summary>Observaciones del docente de punta a punta: idempotencia, control de crédito y liquidación ciega.</summary>
[Collection(ApiCollection.Name)]
public sealed class ProfessorObservationTests(ApiFixture api)
{
    private static async Task<ProductResponse> NewProductAsync(HttpClient admin, int boxes) =>
        await admin.PostAsync<ProductResponse>("/api/products",
            new CreateProductRequest($"PRODUCTO OBS {Guid.NewGuid():N}"[..36], "Prueba", ApiFixture.BeveragesCategoryId,
                100m, 3m, 80m, 36, boxes, 0, 0, 100_000, false, null), HttpStatusCode.Created);

    private static Task<ClientResponse> NewClientAsync(HttpClient http) =>
        http.PostAsync<ClientResponse>("/api/clients",
            new CreateClientRequest($"V-{Random.Shared.Next(10_000_000, 99_999_999)}", "Cliente de prueba", null, null), HttpStatusCode.Created);

    private static CreateSaleRequest Sale(Guid clientId, Guid productId, int boxes, decimal payment, StockOrigin origin = StockOrigin.Warehouse, Guid? truckId = null) =>
        new(clientId, origin, truckId, null, [new LineRequest(productId, boxes, 0)], payment, null, null);

    private static async Task<HttpResponseMessage> PostSaleWithKeyAsync(HttpClient http, CreateSaleRequest sale, Guid key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/invoices/sales") { Content = JsonContent.Create(sale, options: ApiFixture.Json) };
        request.Headers.Add("Idempotency-Key", key.ToString());
        return await http.SendAsync(request);
    }

    [Fact]
    public async Task Sale_RetriedWithTheSameIdempotencyKey_IsCreatedOnlyOnce()
    {
        var employee = await api.EmployeeAsync();
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 10);
        var client = await NewClientAsync(employee);
        var key = Guid.NewGuid();

        var first = await PostSaleWithKeyAsync(employee, Sale(client.Id, product.Id, 1, 100m), key);
        var retry = await PostSaleWithKeyAsync(employee, Sale(client.Id, product.Id, 1, 100m), key);
        var firstInvoice = await first.Content.ReadFromJsonAsync<InvoiceResponse>(ApiFixture.Json);
        var retryInvoice = await retry.Content.ReadFromJsonAsync<InvoiceResponse>(ApiFixture.Json);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Equal(firstInvoice!.Id, retryInvoice!.Id);
        Assert.Equal(324, (await admin.GetAsync<ProductResponse>($"/api/products/{product.Id}")).StockUnits);
    }

    [Fact]
    public async Task CreditLimit_BlocksCreditSales_ButNotCashSales_AndOnlyAdminSetsIt()
    {
        var admin = await api.AdminAsync();
        var employee = await api.EmployeeAsync();
        var product = await NewProductAsync(admin, 10);
        var client = await NewClientAsync(admin);

        var byEmployee = await employee.SendRawAsync(HttpMethod.Put, $"/api/clients/{client.Id}/credit-limit", new SetCreditLimitRequest(50m));
        var limited = await admin.SendAsync<ClientResponse>(HttpMethod.Put, $"/api/clients/{client.Id}/credit-limit", new SetCreditLimitRequest(50m));
        var blocked = await employee.SendRawAsync(HttpMethod.Post, "/api/invoices/sales", Sale(client.Id, product.Id, 1, 20m));
        var problem = JsonDocument.Parse(await blocked.Content.ReadAsStringAsync()).RootElement;
        await employee.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, product.Id, 1, 100m), HttpStatusCode.Created);

        Assert.Equal(HttpStatusCode.Forbidden, byEmployee.StatusCode);
        Assert.Equal(50m, limited.CreditLimit);
        Assert.Equal(HttpStatusCode.BadRequest, blocked.StatusCode);
        Assert.StartsWith("Crédito bloqueado", problem.GetProperty("detail").GetString());
        Assert.Equal(324, (await admin.GetAsync<ProductResponse>($"/api/products/{product.Id}")).StockUnits);
    }

    [Fact]
    public async Task BlindSettlement_ComparesDeclaredCashAndGoods_AndClosesTheRoute()
    {
        var admin = await api.AdminAsync();
        var employee = await api.EmployeeAsync();
        var product = await NewProductAsync(admin, 10);
        var client = await NewClientAsync(employee);
        var truck = await admin.PostAsync<TruckResponse>("/api/trucks", new TruckRequest("Ruta", $"R{Random.Shared.Next(100000, 999999)}", 100), HttpStatusCode.Created);

        var loaded = await employee.PostAsync<TruckResponse>($"/api/trucks/{truck.Id}/load", new LoadTruckRequest(product.Id, TruckLoadMode.Add, 4, 0, null));
        var dispatched = await employee.PostAsync<TruckResponse>($"/api/trucks/{truck.Id}/dispatch", null);
        await employee.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, product.Id, 1, 40m, StockOrigin.Truck, truck.Id), HttpStatusCode.Created);

        // Debía traer 3 cajas (108 und) y $40; declara 2 cajas + 30 und (102 und) y $30.
        var planilla = await employee.PostAsync<RouteSettlementResponse>($"/api/trucks/{truck.Id}/settlement",
            new SettleRouteRequest(30m, [new LineRequest(product.Id, 2, 30)], "Se rompió una gavera"));
        var after = await admin.GetAsync<TruckResponse>($"/api/trucks/{truck.Id}");
        var warehouse = await admin.GetAsync<ProductResponse>($"/api/products/{product.Id}");
        var again = await employee.SendRawAsync(HttpMethod.Post, $"/api/trucks/{truck.Id}/settlement", new SettleRouteRequest(0m, [], null));
        var historyAsEmployee = await employee.SendRawAsync(HttpMethod.Get, $"/api/trucks/settlements?truckId={truck.Id}");
        var history = await admin.GetAsync<List<RouteSettlementResponse>>($"/api/trucks/settlements?truckId={truck.Id}");

        Assert.Equal((Truck.StatusLoading, Truck.StatusOnRoute), (loaded.Status, dispatched.Status));
        Assert.Equal((1, 100m, 40m, 30m, -10m), (planilla.InvoicesCount, planilla.SalesTotal, planilla.ExpectedCash, planilla.CountedCash, planilla.CashDifference));
        var line = Assert.Single(planilla.Lines);
        Assert.Equal((108, 102, -6, -18m), (line.ExpectedUnits, line.CountedUnits, line.DifferenceUnits, line.DifferenceValue));
        Assert.False(planilla.IsBalanced);
        Assert.Equal((Truck.StatusSettled, 0), (after.Status, after.TotalUnits));
        Assert.Equal(216 + 102, warehouse.StockUnits);
        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, historyAsEmployee.StatusCode);
        Assert.Equal(planilla.Id, Assert.Single(history).Id);
    }

    [Fact]
    public async Task Dashboard_IncludesDebtAging()
    {
        var admin = await api.AdminAsync();
        var product = await NewProductAsync(admin, 10);
        var client = await NewClientAsync(admin);
        await admin.PostAsync<InvoiceResponse>("/api/invoices/sales", Sale(client.Id, product.Id, 1, 0m), HttpStatusCode.Created);

        var dashboard = await admin.GetAsync<DashboardResponse>("/api/reports/dashboard");

        Assert.True(dashboard.DebtAging.TotalDebt >= 100m);
        Assert.Equal(30, dashboard.DebtAging.MaxOverdueDays);
        Assert.Equal(4, dashboard.DebtAging.Buckets.Count);
        Assert.True(dashboard.DebtAging.Buckets[0].Amount >= 100m);
    }
}
