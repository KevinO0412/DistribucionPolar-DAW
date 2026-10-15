using Core.Application.Abstractions;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ClientRepository(ApplicationDbContext context) : IClientRepository
{
    public async Task<IReadOnlyList<Client>> SearchAsync(string? search, bool receivableOnly, CancellationToken cancellationToken = default)
    {
        var query = context.Clients.AsNoTracking().Include(c => c.EmptyBalances).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim().Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_")}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.Name, pattern) ||
                EF.Functions.ILike(c.Rif, pattern) ||
                EF.Functions.ILike(c.Address, pattern) ||
                (c.Nickname != null && EF.Functions.ILike(c.Nickname, pattern)));
        }

        if (receivableOnly)
        {
            query = query.Where(c => c.MoneyBalance < 0 || c.EmptyBoxesBalance < 0 || c.EmptyUnitsBalance < 0);
        }

        return await query.OrderBy(c => c.Code).ToListAsync(cancellationToken);
    }

    public Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Clients.AsNoTracking().Include(c => c.EmptyBalances).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<Client?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Clients.Include(c => c.EmptyBalances).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<bool> RifExistsAsync(string rif, CancellationToken cancellationToken = default)
    {
        return context.Clients.AsNoTracking().AnyAsync(c => c.Rif == rif, cancellationToken);
    }

    public async Task<int> GetMaxCodeAsync(CancellationToken cancellationToken = default)
    {
        return await context.Clients.AsNoTracking().IgnoreQueryFilters().MaxAsync(c => (int?)c.Code, cancellationToken) ?? 0;
    }

    public void Add(Client client)
    {
        context.Clients.Add(client);
    }

    public void AddMovement(ClientMovement movement)
    {
        context.ClientMovements.Add(movement);
    }

    public async Task<IReadOnlyList<ClientMovement>> GetMovementsAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await context.ClientMovements
            .AsNoTracking()
            .Where(m => m.ClientId == clientId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OpenSaleRow>> GetOpenSalesAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await OpenSales(context.Invoices.Where(i => i.ClientId == clientId)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<OpenSaleRow>>> GetOpenSalesAsync(IEnumerable<Guid> clientIds, CancellationToken cancellationToken = default)
    {
        var ids = clientIds.Distinct().ToList();
        var rows = await OpenSales(context.Invoices.Where(i => ids.Contains(i.ClientId))).ToListAsync(cancellationToken);
        return rows.GroupBy(r => r.ClientId).ToDictionary(g => g.Key, g => (IReadOnlyList<OpenSaleRow>)g.ToList());
    }

    /// <summary>Proyección a DTO: solo fecha y pendiente de las ventas vigentes que dejaron saldo.</summary>
    private static IQueryable<OpenSaleRow> OpenSales(IQueryable<Invoice> invoices) =>
        invoices
            .AsNoTracking()
            .Where(i => i.Type == InvoiceType.Sale && !i.IsCancelled && i.Pending > 0)
            .OrderByDescending(i => i.IssuedAt)
            .Select(i => new OpenSaleRow(i.ClientId, i.IssuedAt, i.Pending));

    public async Task<IReadOnlyDictionary<Guid, DateTime>> GetLastSaleDatesAsync(IEnumerable<Guid> clientIds, CancellationToken cancellationToken = default)
    {
        var ids = clientIds.Distinct().ToList();
        return await context.Invoices
            .AsNoTracking()
            .Where(i => ids.Contains(i.ClientId) && i.Type == InvoiceType.Sale && !i.IsCancelled)
            .GroupBy(i => i.ClientId)
            .Select(g => new { ClientId = g.Key, Last = g.Max(i => i.IssuedAt) })
            .ToDictionaryAsync(x => x.ClientId, x => x.Last, cancellationToken);
    }
}
