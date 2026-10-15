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
    Task<PagedResponse<InvoiceSummaryResponse>> ListAsync(
        DateTime? fromUtc, DateTime? toUtc, Guid? clientId, InvoiceType? type, bool includeCancelled,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
