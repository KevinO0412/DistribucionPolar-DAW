namespace Core.Application.Abstractions;

/// <summary>
/// Correlativos atómicos en la base de datos (sin choques entre usuarios concurrentes).
/// Debe llamarse dentro de la transacción del proceso: si este se revierte, el número no se consume.
/// </summary>
public interface IDocumentNumberGenerator
{
    /// <param name="sequence">Nombre de la secuencia.</param>
    /// <param name="year">Año (0 si la secuencia no es anual).</param>
    /// <param name="month">Mes (0 si la secuencia no es mensual).</param>
    /// <param name="floor">El número devuelto será siempre mayor que este valor (por ejemplo, el máximo ya existente).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<int> NextAsync(string sequence, int year, int month, int floor = 0, CancellationToken cancellationToken = default);
}

public static class DocumentSequences
{
    public const string Invoice = "invoice";
    public const string Gift = "gift";
    public const string Purchase = "purchase";
    public const string Client = "client";
}
