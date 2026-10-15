using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Abstractions;

/// <summary>Venta ya emitida con una clave Idempotency-Key: hash del cuerpo y factura resultante.</summary>
public sealed record IdempotentSale(string RequestHash, Guid InvoiceId);

public interface IInvoiceRepository
{
    void Add(Invoice invoice);

    /// <summary>Venta registrada con la clave de idempotencia indicada, si existe.</summary>
    Task<IdempotentSale?> FindIdempotentSaleAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Registra la clave junto con la factura; se guarda en la misma transacción que la venta.</summary>
    void AddIdempotentSale(string key, string requestHash, Guid invoiceId, string? username, DateTime createdAtUtc);

    /// <summary>Factura completa con líneas y vacíos (AsNoTrackingWithIdentityResolution).</summary>
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Factura con líneas y vacíos, con seguimiento de cambios.</summary>
    Task<Invoice?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Listado proyectado directamente a DTO con Select (solo las columnas necesarias).</summary>
    Task<PagedResponse<InvoiceSummaryResponse>> ListAsync(
        DateTime? fromUtc, DateTime? toUtc, Guid? clientId, InvoiceType? type, bool includeCancelled,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
