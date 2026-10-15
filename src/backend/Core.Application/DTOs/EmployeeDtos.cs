namespace Core.Application.DTOs;

/// <summary>Empleado de la distribuidora.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Name">Nombre completo.</param>
/// <param name="Role">Cargo.</param>
/// <param name="IsActive">Indica si está activo.</param>
/// <param name="CreatedAt">Fecha de creación (UTC).</param>
public sealed record EmployeeResponse(Guid Id, string Name, string Role, bool IsActive, DateTime CreatedAt);

/// <summary>Alta de empleado (solo Admin).</summary>
/// <param name="Name" example="Carlos Pérez">Nombre, obligatorio, máximo 100 caracteres.</param>
/// <param name="Role" example="Vendedor">Cargo; si se omite, "Empleado".</param>
public sealed record CreateEmployeeRequest(string Name, string? Role);

/// <summary>Edición de empleado (solo Admin). Los empleados no se eliminan: se desactivan.</summary>
/// <param name="Name" example="Carlos Pérez">Nombre, obligatorio.</param>
/// <param name="Role" example="Vendedor">Cargo.</param>
/// <param name="IsActive" example="true">Activo o inactivo.</param>
public sealed record UpdateEmployeeRequest(string Name, string? Role, bool IsActive);
