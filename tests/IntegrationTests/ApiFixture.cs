using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Application.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace IntegrationTests;

/// <summary>
/// Levanta PostgreSQL 15 en un contenedor desechable y la API real en memoria (entorno Development:
/// migraciones, siembra y usuarios de prueba). Se comparte entre todas las pruebas de la colección.
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    public static readonly Guid BeveragesCategoryId = Guid.Parse("b2222222-2222-4222-8222-222222222222");
    public static readonly Guid SeedEmployeeId = Guid.Parse("00000100-0000-4000-8000-000000000001");

    private readonly PostgreSqlContainer database = new PostgreSqlBuilder("postgres:15-alpine").Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = default!;

    public static JsonSerializerOptions Json { get; } = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        Factory = CreateFactory(loginLimit: 10_000);
        using var warmUp = Factory.CreateClient();
        (await warmUp.GetAsync("/health")).EnsureSuccessStatusCode();
    }

    public WebApplicationFactory<Program> CreateFactory(int loginLimit) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:DefaultConnection", database.GetConnectionString());
            builder.UseSetting("RateLimiting:LoginPermitLimit", loginLimit.ToString());
            builder.UseSetting("RateLimiting:GlobalPermitLimit", "100000");
        });

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await database.DisposeAsync();
    }

    public async Task<HttpClient> LoginAsync(string username, string password)
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, password), Json);
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(Json);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    public Task<HttpClient> AdminAsync() => LoginAsync("admin", "Admin2026!");

    public Task<HttpClient> EmployeeAsync() => LoginAsync("empleado", "Empleado2026!");
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "api";
}

internal static class HttpExtensions
{
    public static async Task<T> SendAsync<T>(this HttpClient client, HttpMethod method, string url, object? body = null, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var response = await client.SendRawAsync(method, url, body);
        string content = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == expected, $"{method} {url}: se esperaba {(int)expected} y llegó {(int)response.StatusCode}. {content}");
        return JsonSerializer.Deserialize<T>(content, ApiFixture.Json)!;
    }

    public static Task<HttpResponseMessage> SendRawAsync(this HttpClient client, HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: ApiFixture.Json);
        }

        return client.SendAsync(request);
    }

    public static Task<T> GetAsync<T>(this HttpClient client, string url) => client.SendAsync<T>(HttpMethod.Get, url);

    public static Task<T> PostAsync<T>(this HttpClient client, string url, object? body, HttpStatusCode expected = HttpStatusCode.OK) =>
        client.SendAsync<T>(HttpMethod.Post, url, body, expected);
}
