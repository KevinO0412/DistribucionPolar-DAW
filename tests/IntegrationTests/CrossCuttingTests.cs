using System.Net;
using System.Net.Http.Json;
using Core.Application.DTOs;

namespace IntegrationTests;

/// <summary>CORS y rate limiting.</summary>
[Collection(ApiCollection.Name)]
public sealed class CrossCuttingTests(ApiFixture api)
{
    [Theory]
    [InlineData("http://localhost:5173", true)]
    [InlineData("https://sitio-no-autorizado.com", false)]
    public async Task Cors_OnlyConfiguredOriginsAreAllowed(string origin, bool allowed)
    {
        using var client = api.Factory.CreateClient();
        var preflight = new HttpRequestMessage(HttpMethod.Options, "/api/products");
        preflight.Headers.Add("Origin", origin);
        preflight.Headers.Add("Access-Control-Request-Method", "GET");
        preflight.Headers.Add("Access-Control-Request-Headers", "authorization");

        var response = await client.SendAsync(preflight);

        Assert.Equal(allowed, response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values) && values.Contains(origin));
    }

    [Fact]
    public async Task Login_IsRateLimited()
    {
        await using var limited = api.CreateFactory(loginLimit: 2);
        using var client = limited.CreateClient();
        var wrong = new LoginRequest("admin", "ClaveIncorrecta");

        var statuses = new List<HttpStatusCode>();
        for (int i = 0; i < 3; i++)
        {
            statuses.Add((await client.PostAsJsonAsync("/api/auth/login", wrong, ApiFixture.Json)).StatusCode);
        }

        Assert.Equal([HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.TooManyRequests], statuses);
    }
}
