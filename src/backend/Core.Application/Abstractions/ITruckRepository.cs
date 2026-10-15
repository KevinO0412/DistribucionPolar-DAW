using Core.Domain.Entities;

namespace Core.Application.Abstractions;

public interface ITruckRepository
{
    /// <summary>Camiones con su inventario y los productos (AsNoTracking).</summary>
    Task<IReadOnlyList<Truck>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Camión con su inventario y los productos (AsNoTracking).</summary>
    Task<Truck?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Camión con su inventario, con seguimiento de cambios.</summary>
    Task<Truck?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> PlateExistsAsync(string plate, Guid? excludeId = null, CancellationToken cancellationToken = default);

    void Add(Truck truck);

    void AddLoad(TruckLoad load);

    /// <summary>Historial de cargas, de la más reciente a la más antigua (AsNoTracking).</summary>
    Task<IReadOnlyList<TruckLoad>> GetLoadsAsync(Guid? truckId, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);
}
