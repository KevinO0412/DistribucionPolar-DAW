using Core.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

/// <summary>
/// Reloj del negocio. "Hoy" y los rangos de los reportes se calculan en la zona horaria de la empresa
/// (configuración Business:TimeZone; por defecto America/Caracas, UTC−4 sin horario de verano).
/// </summary>
public sealed class BusinessClock(TimeProvider timeProvider, IConfiguration configuration) : IClock
{
    private readonly TimeZoneInfo zone = ResolveZone(configuration["Business:TimeZone"] ?? "America/Caracas");

    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    public DateOnly Today => DateOnly.FromDateTime(ToLocal(UtcNow));

    public (DateTime StartUtc, DateTime EndUtc) GetDayRangeUtc(DateOnly date)
    {
        var start = TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), zone);
        var end = TimeZoneInfo.ConvertTimeToUtc(date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), zone);
        return (DateTime.SpecifyKind(start, DateTimeKind.Utc), DateTime.SpecifyKind(end, DateTimeKind.Utc));
    }

    public DateTime ToLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);

    private static TimeZoneInfo ResolveZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            // Respaldo si el sistema no trae la base de zonas horarias: UTC−4 fijo.
            return TimeZoneInfo.CreateCustomTimeZone(id, TimeSpan.FromHours(-4), id, id);
        }
    }
}
