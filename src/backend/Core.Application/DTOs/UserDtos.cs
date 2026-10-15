using Core.Domain.Enums;

namespace Core.Application.DTOs;

/// <summary>Usuario del sistema.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Username">Username.</param>
/// <param name="Email">Email.</param>
/// <param name="FullName">Nombre completo.</param>
/// <param name="Role">Admin o Employee.</param>
/// <param name="IsActive">Activo.</param>
/// <param name="AttendantName">Vendedor fijo asignado.</param>
/// <param name="CanChangeAttendant">Puede elegir el vendedor al facturar.</param>
/// <param name="CreatedAt">Fecha de alta (UTC).</param>
public sealed record UserResponse(
    Guid Id, string Username, string Email, string FullName, UserRole Role, bool IsActive, string? AttendantName, bool CanChangeAttendant, DateTime CreatedAt);

/// <summary>Alta de usuario (solo Admin).</summary>
/// <param name="Username" example="vendedor1">Username único.</param>
/// <param name="Email" example="vendedor1@distribucionpolar.local">Email único.</param>
/// <param name="FullName" example="Carlos Pérez">Nombre.</param>
/// <param name="Password" example="Clave2026!">Clave inicial, mínimo 6 caracteres.</param>
/// <param name="Role" example="Employee">Admin o Employee.</param>
/// <param name="AttendantName" example="Carlos Pérez">Vendedor fijo (si no puede elegir).</param>
/// <param name="CanChangeAttendant" example="false">Puede elegir el vendedor al facturar.</param>
public sealed record CreateUserRequest(
    string Username, string Email, string FullName, string Password, UserRole Role, string? AttendantName, bool CanChangeAttendant);

/// <summary>Edición de usuario (solo Admin).</summary>
/// <param name="Email">Email único.</param>
/// <param name="FullName">Nombre.</param>
/// <param name="Role">Admin o Employee.</param>
/// <param name="IsActive">Activo.</param>
/// <param name="AttendantName">Vendedor fijo.</param>
/// <param name="CanChangeAttendant">Puede elegir el vendedor.</param>
public sealed record UpdateUserRequest(string Email, string FullName, UserRole Role, bool IsActive, string? AttendantName, bool CanChangeAttendant);

/// <summary>Restablecimiento de clave (solo Admin).</summary>
/// <param name="NewPassword" example="Nueva2026!">Nueva clave, mínimo 6 caracteres.</param>
public sealed record ResetPasswordRequest(string NewPassword);

/// <summary>Cambio de la clave propia.</summary>
/// <param name="CurrentPassword">Clave actual.</param>
/// <param name="NewPassword">Nueva clave, mínimo 6 caracteres y distinta de la actual.</param>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>Registro de auditoría.</summary>
/// <param name="Id">Identificador (BIGINT).</param>
/// <param name="Action">Acción (invoice_created, client_payment…).</param>
/// <param name="Module">Módulo.</param>
/// <param name="Description">Descripción.</param>
/// <param name="EntityName">Entidad afectada.</param>
/// <param name="EntityId">Identificador de la entidad.</param>
/// <param name="Amount">Monto, si aplica.</param>
/// <param name="BeforeJson">Estado anterior (JSON).</param>
/// <param name="AfterJson">Estado posterior (JSON).</param>
/// <param name="MetadataJson">Datos adicionales (JSON).</param>
/// <param name="Username">Usuario.</param>
/// <param name="Role">Rol.</param>
/// <param name="IpAddress">Dirección IP de origen.</param>
/// <param name="CreatedAt">Fecha (UTC).</param>
public sealed record AuditLogResponse(
    long Id, string Action, string Module, string Description, string? EntityName, string? EntityId, decimal? Amount,
    string? BeforeJson, string? AfterJson, string? MetadataJson, string? Username, string? Role, string? IpAddress, DateTime CreatedAt);
