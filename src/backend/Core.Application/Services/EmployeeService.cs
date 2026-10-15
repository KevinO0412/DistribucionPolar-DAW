using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Entities;
using FluentValidation;

namespace Core.Application.Services;

public sealed class EmployeeService(
    IEmployeeRepository employees,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    IValidator<CreateEmployeeRequest> createValidator,
    IValidator<UpdateEmployeeRequest> updateValidator) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var list = await employees.GetAllAsync(includeInactive, cancellationToken);
        return list.Select(e => e.ToResponse()).ToList();
    }

    public async Task<EmployeeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await employees.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el empleado {id}.");
        return employee.ToResponse();
    }

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var employee = new Employee(request.Name, request.Role);
        employees.Add(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("employee_created", "employees", $"Empleado creado: {employee.Name}",
            nameof(Employee), employee.Id.ToString(), after: employee.ToResponse(), cancellationToken: cancellationToken);
        return employee.ToResponse();
    }

    public async Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var employee = await employees.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el empleado {id}.");
        var before = employee.ToResponse();

        employee.Update(request.Name, request.Role);
        if (request.IsActive)
        {
            employee.Activate();
        }
        else
        {
            employee.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("employee_updated", "employees", $"Empleado actualizado: {employee.Name}",
            nameof(Employee), employee.Id.ToString(), before: before, after: employee.ToResponse(), cancellationToken: cancellationToken);
        return employee.ToResponse();
    }
}
