namespace Core.Application.Abstractions;

/// <summary>Reloj del negocio: "hoy" se calcula en la zona horaria de la empresa, no en UTC.</summary>
public interface IClock
{
    DateTime UtcNow { get; }

    /// <summary>Fecha local de la empresa.</summary>
    DateOnly Today { get; }

    /// <summary>Rango [inicio, fin) en UTC que corresponde a un día local de la empresa.</summary>
    (DateTime StartUtc, DateTime EndUtc) GetDayRangeUtc(DateOnly date);

    /// <summary>Convierte un instante UTC a la hora local de la empresa.</summary>
    DateTime ToLocal(DateTime utc);
}
