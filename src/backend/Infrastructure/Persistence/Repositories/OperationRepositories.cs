using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class PurchaseRepository(ApplicationDbContext context) : IPurchaseRepository
{
    public void Add(Purchase purchase)
    {
        context.Purchases.Add(purchase);
    }

    public Task<Purchase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Purchases.AsNoTracking().Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Purchase>> ListAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        return await context.Purchases
            .AsNoTracking()
            .Include(p => p.Items)
            .Where(p => fromUtc == null || p.PurchasedAt >= fromUtc)
            .Where(p => toUtc == null || p.PurchasedAt < toUtc)
            .OrderByDescending(p => p.PurchasedAt)
            .ToListAsync(cancellationToken);
    }
}

public sealed class DamagedProductRepository(ApplicationDbContext context) : IDamagedProductRepository
{
    public void Add(DamagedProduct damage)
    {
        context.DamagedProducts.Add(damage);
    }

    public async Task<IReadOnlyList<DamagedProduct>> ListAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        return await context.DamagedProducts
            .AsNoTracking()
            .Where(d => fromUtc == null || d.OccurredAt >= fromUtc)
            .Where(d => toUtc == null || d.OccurredAt < toUtc)
            .OrderByDescending(d => d.OccurredAt)
            .ToListAsync(cancellationToken);
    }
}

public sealed class InternalConsumptionRepository(ApplicationDbContext context) : IInternalConsumptionRepository
{
    public void Add(InternalConsumption consumption)
    {
        context.InternalConsumptions.Add(consumption);
    }

    public async Task<IReadOnlyList<InternalConsumption>> ListAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        return await context.InternalConsumptions
            .AsNoTracking()
            .Where(c => fromUtc == null || c.OccurredAt >= fromUtc)
            .Where(c => toUtc == null || c.OccurredAt < toUtc)
            .OrderByDescending(c => c.OccurredAt)
            .ToListAsync(cancellationToken);
    }
}

public sealed class ConsignmentRepository(ApplicationDbContext context) : IConsignmentRepository
{
    public void Add(ConsignmentEvent consignmentEvent)
    {
        context.ConsignmentEvents.Add(consignmentEvent);
    }

    public Task<ConsignmentEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.ConsignmentEvents.AsNoTracking().Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<ConsignmentEvent?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.ConsignmentEvents.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ConsignmentEvent>> ListAsync(bool? isClosed, CancellationToken cancellationToken = default)
    {
        return await context.ConsignmentEvents
            .AsNoTracking()
            .Include(c => c.Items)
            .Where(c => isClosed == null || c.IsClosed == isClosed)
            .OrderByDescending(c => c.EventDate)
            .ToListAsync(cancellationToken);
    }
}

public sealed class AuditLogRepository(ApplicationDbContext context) : IAuditLogRepository
{
    public async Task<PagedResponse<AuditLogResponse>> ListAsync(
        DateTime? fromUtc, DateTime? toUtc, string? module, string? username, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.AuditLogs
            .AsNoTracking()
            .Where(a => fromUtc == null || a.CreatedAt >= fromUtc)
            .Where(a => toUtc == null || a.CreatedAt < toUtc)
            .Where(a => module == null || a.Module == module)
            .Where(a => username == null || a.Username == username);

        int total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(a => new AuditLogResponse(
            a.Id, a.Action, a.Module, a.Description, a.EntityName, a.EntityId, a.Amount,
            a.BeforeJson, a.AfterJson, a.MetadataJson, a.Username, a.Role, a.IpAddress?.ToString(), a.CreatedAt)).ToList();
        return new PagedResponse<AuditLogResponse>(items, page, pageSize, total);
    }
}
