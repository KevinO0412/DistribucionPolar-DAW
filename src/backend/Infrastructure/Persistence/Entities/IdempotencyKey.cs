namespace Infrastructure.Persistence.Entities;

/// <summary>
/// Clave de idempotencia de una venta (encabezado Idempotency-Key). Guarda el hash del cuerpo y la factura
/// emitida para que un reintento con la misma clave devuelva esa factura en lugar de emitir otra.
/// </summary>
public sealed class IdempotencyKey
{
    public const int KeyMaxLength = 100;

    public string Key { get; set; } = default!;

    /// <summary>SHA-256 en hexadecimal del cuerpo de la petición.</summary>
    public string RequestHash { get; set; } = default!;

    public Guid InvoiceId { get; set; }

    public string? Username { get; set; }

    public DateTime CreatedAt { get; set; }
}
