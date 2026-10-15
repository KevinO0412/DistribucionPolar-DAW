namespace Core.Application.Abstractions;

/// <summary>Tasas de cambio de referencia del dólar.</summary>
/// <param name="Base">Moneda base (USD).</param>
/// <param name="Rates">Tasas por moneda (VES, COP…).</param>
/// <param name="UpdatedAt">Fecha de actualización informada por el proveedor (UTC).</param>
/// <param name="Source">Origen del dato: "live" (proveedor externo) o "cache" (último valor conocido).</param>
public sealed record ExchangeRates(string Base, IReadOnlyDictionary<string, decimal> Rates, DateTime UpdatedAt, string Source);

/// <summary>
/// Proveedor externo de tasas de cambio. Su implementación está protegida con reintentos,
/// timeout y Circuit Breaker, y cae al último valor conocido si el proveedor no responde.
/// </summary>
public interface IExchangeRateProvider
{
    Task<ExchangeRates> GetLatestAsync(CancellationToken cancellationToken = default);
}
