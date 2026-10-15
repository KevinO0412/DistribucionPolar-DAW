using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>Salida de mercancía para consumo interno o como regalía a un empleado.</summary>
public sealed class InternalConsumption : StockWithdrawal
{
    // Constructor para EF Core.
    private InternalConsumption()
    {
    }

    public InternalConsumption(
        Product product,
        BoxQuantity quantity,
        StockOrigin origin,
        Truck? truck,
        Employee employee,
        InternalConsumptionType type,
        string? observation,
        DateTime occurredAt)
        : base(product, quantity, origin, truck, observation, occurredAt)
    {
        Type = Enum.IsDefined(type) ? type : throw new DomainException("Tipo de consumo inválido.");
        EmployeeId = employee.Id;
        EmployeeName = employee.Name;
    }

    public InternalConsumptionType Type { get; private set; }

    public Guid EmployeeId { get; private set; }

    public string EmployeeName { get; private set; } = default!;
}
