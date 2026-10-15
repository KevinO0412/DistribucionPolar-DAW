namespace Core.Application.Abstractions;

/// <summary>Respaldo de datos para administración.</summary>
public interface IBackupExporter
{
    /// <summary>Exporta todas las tablas del negocio en un documento JSON.</summary>
    Task<byte[]> ExportJsonAsync(CancellationToken cancellationToken = default);

    /// <summary>Exporta un conjunto de datos en CSV: "products", "clients", "invoices" o "movements".</summary>
    Task<byte[]> ExportCsvAsync(string dataset, CancellationToken cancellationToken = default);
}
