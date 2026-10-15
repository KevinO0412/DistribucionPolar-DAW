using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Core.Application.Abstractions;
using Core.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Infrastructure.Services;

public sealed class ExchangeRateOptions
{
    public const string SectionName = "ExchangeRates";

    /// <summary>URL del proveedor público (sin clave) con las tasas del USD.</summary>
    public string Url { get; init; } = "https://open.er-api.com/v6/latest/USD";

    /// <summary>Monedas que se exponen (sin valor inicial: el binder de configuración agrega a los arreglos, no los reemplaza).</summary>
    public string[] Currencies { get; init; } = [];

    public IReadOnlyList<string> EffectiveCurrencies =>
        Currencies.Length == 0 ? ["VES", "COP", "EUR"] : Currencies.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Minutos que se reutiliza una respuesta exitosa antes de volver a consultar.</summary>
    public int CacheMinutes { get; init; } = 10;
}

/// <summary>Último valor conocido, compartido por toda la aplicación (Singleton).</summary>
public sealed class ExchangeRateCache
{
    private readonly Lock gate = new();
    private (ExchangeRates Rates, DateTime FetchedAt)? last;

    public (ExchangeRates Rates, DateTime FetchedAt)? Last
    {
        get { lock (gate) { return last; } }
    }

    public void Store(ExchangeRates rates, DateTime fetchedAt)
    {
        lock (gate) { last = (rates, fetchedAt); }
    }
}

/// <summary>
/// Cliente HTTP del proveedor de tasas. El HttpClient tiene una canalización de resiliencia (Polly):
/// timeout por intento, reintentos con espera exponencial y Circuit Breaker. Si el proveedor falla o el
/// circuito está abierto, se responde con el último valor conocido (fallback); si no existe, 503.
/// </summary>
public sealed class ExchangeRateProvider(
    HttpClient httpClient,
    ExchangeRateCache cache,
    IOptions<ExchangeRateOptions> options,
    TimeProvider timeProvider,
    ILogger<ExchangeRateProvider> logger) : IExchangeRateProvider
{
    private readonly ExchangeRateOptions settings = options.Value;

    public async Task<ExchangeRates> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (cache.Last is { } fresh && now - fresh.FetchedAt < TimeSpan.FromMinutes(settings.CacheMinutes))
        {
            return fresh.Rates;
        }

        try
        {
            var response = await httpClient.GetFromJsonAsync<ProviderResponse>(settings.Url, cancellationToken)
                ?? throw new HttpRequestException("Respuesta vacía del proveedor de tasas.");
            if (!string.Equals(response.Result, "success", StringComparison.OrdinalIgnoreCase) || response.Rates is null)
            {
                throw new HttpRequestException("El proveedor de tasas devolvió un error.");
            }

            var rates = settings.EffectiveCurrencies
                .Where(response.Rates.ContainsKey)
                .ToDictionary(c => c, c => Math.Round(response.Rates[c], 4));
            var result = new ExchangeRates(response.BaseCode ?? "USD", rates,
                DateTimeOffset.FromUnixTimeSeconds(response.TimeLastUpdateUnix).UtcDateTime, "live");
            cache.Store(result, now);
            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException or BrokenCircuitException or TimeoutRejectedException or System.Text.Json.JsonException or TaskCanceledException
                                   && !cancellationToken.IsCancellationRequested)
        {
            if (cache.Last is { } stale)
            {
                logger.LogWarning(ex, "Proveedor de tasas no disponible; se usa el último valor conocido.");
                return stale.Rates with { Source = "cache" };
            }

            throw new ExternalServiceUnavailableException(
                "El servicio de tasas de cambio no está disponible y no hay un valor previo. Intente más tarde.", ex);
        }
    }

    private sealed record ProviderResponse(
        [property: JsonPropertyName("result")] string? Result,
        [property: JsonPropertyName("base_code")] string? BaseCode,
        [property: JsonPropertyName("time_last_update_unix")] long TimeLastUpdateUnix,
        [property: JsonPropertyName("rates")] Dictionary<string, decimal>? Rates);
}
