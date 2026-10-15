using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(ApplicationDbContext context) : IInvoiceRepository
{
    public void Add(Invoice invoice)
    {
        context.Invoices.Add(invoice);
    }

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Invoices
            .AsNoTrackingWithIdentityResolution()
            .Include(i => i.Items)
            .Include(i => i.EmptyGroups)
            .AsSplitQuery()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public Task<Invoice?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Invoices
            .Include(i => i.Items)
            .Include(i => i.EmptyGroups)
            .AsSplitQuery()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    /// <summary>Proyección a DTO con Select: PostgreSQL devuelve solo las columnas del listado.</summary>
    public async Task<PagedResponse<InvoiceSummaryResponse>> ListAsync(
        DateTime? fromUtc, DateTime? toUtc, Guid? clientId, InvoiceType? type, bool includeCancelled,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Invoices
            .AsNoTracking()
            .Where(i => fromUtc == null || i.IssuedAt >= fromUtc)
            .Where(i => toUtc == null || i.IssuedAt < toUtc)
            .Where(i => clientId == null || i.ClientId == clientId)
            .Where(i => type == null || i.Type == type)
            .Where(i => includeCancelled || !i.IsCancelled);

        int total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(i => i.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InvoiceSummaryResponse(
                i.Id, i.Number, i.Type, i.ClientId, i.ClientName, i.IssuedAt, i.DispatchOrigin,
                i.AttendantName, i.Total, i.Payment, i.Pending, i.IsCancelled))
            .ToListAsync(cancellationToken);

        return new PagedResponse<InvoiceSummaryResponse>(items, page, pageSize, total);
    }
}
