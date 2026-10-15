using Core.Application.DTOs;
using Core.Domain.Entities;

namespace Core.Application.Abstractions;

public interface IPurchaseRepository
{
    void Add(Purchase purchase);

    Task<Purchase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Purchase>> ListAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);
}

public interface IDamagedProductRepository
{
    void Add(DamagedProduct damage);

    Task<IReadOnlyList<DamagedProduct>> ListAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);
}

public interface IInternalConsumptionRepository
{
    void Add(InternalConsumption consumption);

    Task<IReadOnlyList<InternalConsumption>> ListAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);
}

public interface IConsignmentRepository
{
    void Add(ConsignmentEvent consignmentEvent);

    Task<ConsignmentEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ConsignmentEvent?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConsignmentEvent>> ListAsync(bool? isClosed, CancellationToken cancellationToken = default);
}

public interface IAuditLogRepository
{
    Task<PagedResponse<AuditLogResponse>> ListAsync(
        DateTime? fromUtc, DateTime? toUtc, string? module, string? username, int page, int pageSize, CancellationToken cancellationToken = default);
}
