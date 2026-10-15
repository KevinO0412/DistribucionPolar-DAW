using Core.Domain.Entities;

namespace Core.Application.Abstractions;

public interface IClientRepository
{
    /// <summary>Busca por nombre, RIF, dirección o referencia (AsNoTracking, con saldos por grupo).</summary>
    Task<IReadOnlyList<Client>> SearchAsync(string? search, bool receivableOnly, CancellationToken cancellationToken = default);

    /// <summary>Cliente con saldos por grupo (AsNoTracking).</summary>
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Cliente con saldos por grupo, con seguimiento de cambios.</summary>
    Task<Client?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> RifExistsAsync(string rif, CancellationToken cancellationToken = default);

    /// <summary>Mayor código asignado, incluidos los clientes eliminados.</summary>
    Task<int> GetMaxCodeAsync(CancellationToken cancellationToken = default);

    void Add(Client client);

    void AddMovement(ClientMovement movement);

    /// <summary>Movimientos del cliente, del más reciente al más antiguo (AsNoTracking).</summary>
    Task<IReadOnlyList<ClientMovement>> GetMovementsAsync(Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>Fecha de la última factura de venta no anulada de cada cliente indicado.</summary>
    Task<IReadOnlyDictionary<Guid, DateTime>> GetLastSaleDatesAsync(IEnumerable<Guid> clientIds, CancellationToken cancellationToken = default);
}
