using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Abstractions;

public interface IInvoiceRepository
{
    void Add(Invoice invoice);

    /// <summary>Factura completa con líneas y vacíos (AsNoTrackingWithIdentityResolution).</summary>
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Factura con líneas y vacíos, con seguimiento de cambios.</summary>
    Task<Invoice?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Listado proyectado directamente a DTO con Select (solo las columnas necesarias).</summary>
    /// <summary>Factura emitida con esa clave de idempotencia, si existe (AsNoTracking, con líneas).</summary>
    Task<Invoice?> GetByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken = default);

    /// <summary>Ventas no anuladas del cliente con saldo pendiente &gt; 0 (para la antigüedad de su deuda).</summary>
    Task<IReadOnlyList<Core.Domain.Services.PendingSale>> GetPendingSalesAsync(Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>Ventas no anuladas despachadas desde el camión desde <paramref name="sinceUtc"/>: cantidad, total y abonos.</summary>
    Task<(int Count, decimal Total, decimal Payments)> GetTruckRouteSalesAsync(Guid truckId, DateTime sinceUtc, CancellationToken cancellationToken = default);

    Task<PagedResponse<InvoiceSummaryResponse>> ListAsync(
        DateTime? fromUtc, DateTime? toUtc, Guid? clientId, InvoiceType? type, bool includeCancelled,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
