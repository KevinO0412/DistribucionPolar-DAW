using Core.Domain.Entities;

namespace Core.Application.Abstractions;

public interface IEmployeeRepository
{
    /// <summary>Empleados ordenados por nombre (AsNoTracking). Por defecto solo los activos.</summary>
    Task<IReadOnlyList<Employee>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default);

    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Employee?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(Employee employee);
}
