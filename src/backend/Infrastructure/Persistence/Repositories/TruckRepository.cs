using Core.Application.Abstractions;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class TruckRepository(ApplicationDbContext context) : ITruckRepository
{
    public async Task<IReadOnlyList<Truck>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await WithStock()
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Truck?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return WithStock().AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public Task<Truck?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Trucks.Include(t => t.Stock).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public Task<bool> PlateExistsAsync(string plate, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return context.Trucks.AsNoTracking().AnyAsync(t => t.Plate == plate && t.Id != excludeId, cancellationToken);
    }

    public void Add(Truck truck)
    {
        context.Trucks.Add(truck);
    }

    public void AddLoad(TruckLoad load)
    {
        context.TruckLoads.Add(load);
    }

    public async Task<IReadOnlyList<TruckLoad>> GetLoadsAsync(Guid? truckId, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        return await context.TruckLoads
            .AsNoTracking()
            .Where(l => truckId == null || l.TruckId == truckId)
            .Where(l => fromUtc == null || l.CreatedAt >= fromUtc)
            .Where(l => toUtc == null || l.CreatedAt < toUtc)
            .OrderByDescending(l => l.CreatedAt)
            .Take(500)
            .ToListAsync(cancellationToken);
    }

    // Los productos (y su categoría) se repiten entre camiones: AsNoTrackingWithIdentityResolution evita duplicarlos en memoria.
    private IQueryable<Truck> WithStock() =>
        context.Trucks.Include(t => t.Stock).ThenInclude(s => s.Product!).ThenInclude(p => p.Category);
}
