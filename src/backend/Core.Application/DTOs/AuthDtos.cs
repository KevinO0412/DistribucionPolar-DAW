namespace Core.Application.DTOs;

/// <summary>Credenciales de acceso.</summary>
/// <param name="Username" example="admin">Username o email del usuario.</param>
/// <param name="Password" example="Admin2026!">Clave, mínimo 6 caracteres.</param>
public sealed record LoginRequest(string Username, string Password);

/// <summary>Resultado de un inicio de sesión exitoso.</summary>
/// <param name="Token">JWT firmado con HMAC-SHA256. Enviar en el encabezado Authorization: Bearer {token}.</param>
/// <param name="TokenType">Tipo de token: siempre "Bearer".</param>
/// <param name="ExpiresAt">Fecha de expiración del token (UTC).</param>
/// <param name="Username">Username normalizado.</param>
/// <param name="Email">Email del usuario.</param>
/// <param name="FullName">Nombre completo.</param>
/// <param name="Role">Rol: Admin o Employee.</param>
public sealed record AuthResponse(
    string Token,
    string TokenType,
    DateTime ExpiresAt,
    string Username,
    string Email,
    string FullName,
    string Role);

/// <summary>Datos del usuario autenticado, leídos de los claims del token.</summary>
/// <param name="Id">Identificador del usuario (claim sub).</param>
/// <param name="Username">Username (claim unique_name).</param>
/// <param name="Email">Email (claim email).</param>
/// <param name="FullName">Nombre completo (claim name).</param>
/// <param name="Role">Rol (claim role).</param>
public sealed record CurrentUserResponse(string? Id, string? Username, string? Email, string? FullName, string? Role);
