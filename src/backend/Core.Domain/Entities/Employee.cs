using Core.Domain.Common;

namespace Core.Domain.Entities;

/// <summary>
/// Empleado de la distribuidora (vendedores, despachadores…). No se elimina: se desactiva para conservar
/// la trazabilidad de las operaciones en las que participó.
/// </summary>
public sealed class Employee : BaseEntity
{
    public const int NameMaxLength = 100;
    public const int RoleMaxLength = 50;
    public const string DefaultRole = "Empleado";

    // Constructor para EF Core.
    private Employee()
    {
    }

    public Employee(string name, string? role)
    {
        Apply(name, role);
        IsActive = true;
    }

    public string Name { get; private set; } = default!;

    public string Role { get; private set; } = default!;

    public bool IsActive { get; private set; }

    public void Update(string name, string? role)
    {
        Apply(name, role);
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    private void Apply(string name, string? role)
    {
        Name = Guard.RequiredText(name, "nombre del empleado", NameMaxLength);
        Role = Guard.OptionalText(role, "cargo", RoleMaxLength) ?? DefaultRole;
    }
}
