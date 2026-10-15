using Core.Domain.Entities;

namespace Core.Application.Abstractions;

/// <summary>Ventas vigentes de un camión desde una fecha: facturas, total facturado y cobrado al facturar.</summary>
public sealed record TruckTripSales(int Invoices, decimal Total, decimal Collected)
{
    public static readonly TruckTripSales None = new(0, 0m, 0m);
}

public interface ITruckRepository
{
    /// <summary>Ventas (no anuladas) de cada camión desde la fecha indicada para ese camión.</summary>
    Task<IReadOnlyDictionary<Guid, TruckTripSales>> GetTripSalesAsync(IReadOnlyDictionary<Guid, DateTime> fromUtcByTruck, CancellationToken cancellationToken = default);

    /// <summary>Camiones con su inventario y los productos (AsNoTracking).</summary>
    Task<IReadOnlyList<Truck>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Camión con su inventario y los productos (AsNoTracking).</summary>
    Task<Truck?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Camión con su inventario, con seguimiento de cambios.</summary>
    Task<Truck?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> PlateExistsAsync(string plate, Guid? excludeId = null, CancellationToken cancellationToken = default);

    void Add(Truck truck);

    void AddLoad(TruckLoad load);

    void AddSettlement(TruckSettlement settlement);

    /// <summary>Liquidación con sus líneas (AsNoTracking).</summary>
    Task<TruckSettlement?> GetSettlementAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Liquidaciones, de la más reciente a la más antigua (máximo 100), con sus líneas.</summary>
    Task<IReadOnlyList<TruckSettlement>> GetSettlementsAsync(Guid? truckId, CancellationToken cancellationToken = default);

    /// <summary>Historial de cargas, de la más reciente a la más antigua (AsNoTracking).</summary>
    Task<IReadOnlyList<TruckLoad>> GetLoadsAsync(Guid? truckId, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);
}
