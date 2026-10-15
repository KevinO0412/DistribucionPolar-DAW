using Core.Application.Abstractions;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository(ApplicationDbContext context) : IEmployeeRepository
{
    public async Task<IReadOnlyList<Employee>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        return await context.Employees
            .AsNoTracking()
            .Where(e => includeInactive || e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public Task<Employee?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public void Add(Employee employee)
    {
        context.Employees.Add(employee);
    }
}
