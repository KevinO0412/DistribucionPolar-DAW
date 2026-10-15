using Core.Application.Abstractions;
using Core.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Presentation.API.Security;

namespace Presentation.API.Controllers;

/// <summary>Autenticación stateless con JWT.</summary>
[ApiController]
[Route("api/auth")]
[Tags("Autenticación")]
[Produces("application/json")]
public sealed class AuthController(IAuthService authService, IUserService userService) : ControllerBase
{
    /// <summary>Inicia sesión y emite un JWT.</summary>
    /// <remarks>
    /// Acepta username o email. El token se firma con HMAC-SHA256 y contiene los claims
    /// sub, unique_name, email, name, role, jti, iss, aud y exp.
    ///
    /// Usuarios de desarrollo: admin / Admin2026! (Admin) y empleado / Empleado2026! (Employee).
    /// </remarks>
    /// <param name="request">Credenciales.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Credenciales válidas: devuelve el token y los datos del usuario.</response>
    /// <response code="400">Datos de entrada inválidos.</response>
    /// <response code="401">Usuario o clave incorrectos, o usuario inactivo.</response>
    /// <response code="429">Demasiados intentos de inicio de sesión desde la misma IP.</response>
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.LoginPolicy)]
    [HttpPost("login")]
    [Consumes("application/json")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests, "application/problem+json")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return Ok(await authService.LoginAsync(request, cancellationToken));
    }

    /// <summary>Devuelve el usuario autenticado.</summary>
    /// <remarks>Lee los datos de los claims del JWT enviado en el encabezado Authorization.</remarks>
    /// <response code="200">Datos del usuario del token.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    [HttpGet("me")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public ActionResult<CurrentUserResponse> Me()
    {
        return Ok(new CurrentUserResponse(
            User.FindFirst("sub")?.Value,
            User.Identity?.Name,
            User.FindFirst("email")?.Value,
            User.FindFirst("name")?.Value,
            User.FindFirst("role")?.Value));
    }

    /// <summary>Cambia la clave del usuario autenticado.</summary>
    /// <param name="request">Clave actual y nueva clave.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Clave cambiada.</response>
    /// <response code="400">Clave actual incorrecta o nueva clave inválida.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await userService.ChangeOwnPasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
