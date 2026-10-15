using Core.Application.Abstractions;
using Core.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.API.Security;

namespace Presentation.API.Controllers;

/// <summary>Reportes, auditoría y respaldo (solo Admin).</summary>
[ApiController]
[Route("api/reports")]
[Tags("Reportes y auditoría")]
[Produces("application/json")]
[Authorize(Roles = Roles.Admin)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class ReportsController(IReportService reports, IBackupExporter backup) : ControllerBase
{
    /// <summary>Reporte diario / cierre de caja (excluye documentos anulados).</summary>
    /// <param name="date">Día local de la empresa (por defecto hoy).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Indicadores del día.</response>
    [HttpGet("daily")]
    [ProducesResponseType<DailyReportResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DailyReportResponse>> Daily([FromQuery] DateOnly? date, CancellationToken cancellationToken) =>
        Ok(await reports.GetDailyReportAsync(date, cancellationToken));

    /// <summary>Balance: dinero y vacíos en la calle, inventario total y consignación por cobrar.</summary>
    /// <response code="200">Balance.</response>
    [HttpGet("balance")]
    [ProducesResponseType<BalanceResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<BalanceResponse>> Balance(CancellationToken cancellationToken) =>
        Ok(await reports.GetBalanceAsync(cancellationToken));

    /// <summary>
    /// Dashboard KPI: valorización costo/venta, alertas MinStock/MaxStock, ventas por día y categoría,
    /// rotación por producto y ocupación de la flota.
    /// </summary>
    /// <param name="from">Desde (fecha local, inclusive). Por defecto, 29 días antes de <paramref name="to"/>.</param>
    /// <param name="to">Hasta (fecha local, inclusive). Por defecto, hoy.</param>
    /// <param name="categoryId">Filtra productos y ventas por categoría.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Indicadores del rango.</response>
    /// <response code="400">Rango de fechas inválido (inicio posterior al fin o más de 366 días).</response>
    [HttpGet("dashboard")]
    [ProducesResponseType<DashboardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<DashboardResponse>> Dashboard(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] Guid? categoryId, CancellationToken cancellationToken) =>
        Ok(await reports.GetDashboardAsync(from, to, categoryId, cancellationToken));

    /// <summary>Bitácora de auditoría (más reciente primero).</summary>
    /// <param name="from">Desde (fecha local, inclusive).</param>
    /// <param name="to">Hasta (fecha local, inclusive).</param>
    /// <param name="module">Filtra por módulo (invoices, clients, trucks…).</param>
    /// <param name="username">Filtra por usuario.</param>
    /// <param name="page">Página (desde 1).</param>
    /// <param name="pageSize">Tamaño de página (1–200).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Página de registros.</response>
    [HttpGet("audit")]
    [ProducesResponseType<PagedResponse<AuditLogResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<AuditLogResponse>>> Audit(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string? module, [FromQuery] string? username,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default) =>
        Ok(await reports.GetAuditLogAsync(from, to, module, username, page, pageSize, cancellationToken));

    /// <summary>Respaldo completo de los datos en JSON.</summary>
    /// <response code="200">Archivo JSON.</response>
    [HttpGet("backup/json")]
    [Produces("application/json")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/json")]
    public async Task<IActionResult> BackupJson(CancellationToken cancellationToken) =>
        File(await backup.ExportJsonAsync(cancellationToken), "application/json", $"respaldo-{DateTime.UtcNow:yyyyMMdd-HHmm}.json");

    /// <summary>Exporta un conjunto de datos en CSV (separador ";").</summary>
    /// <param name="dataset" example="products">products, clients, invoices o movements.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Archivo CSV.</response>
    /// <response code="400">Conjunto no válido.</response>
    [HttpGet("backup/csv/{dataset}")]
    [Produces("text/csv")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "text/csv")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<IActionResult> BackupCsv(string dataset, CancellationToken cancellationToken) =>
        File(await backup.ExportCsvAsync(dataset, cancellationToken), "text/csv", $"{dataset}-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv");
}

/// <summary>Usuarios del sistema (solo Admin).</summary>
[ApiController]
[Route("api/users")]
[Tags("Usuarios")]
[Produces("application/json")]
[Authorize(Roles = Roles.Admin)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class UsersController(IUserService users) : ControllerBase
{
    /// <summary>Lista los usuarios.</summary>
    /// <response code="200">Usuarios.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await users.GetAllAsync(cancellationToken));

    /// <summary>Crea un usuario.</summary>
    /// <param name="request">Datos del usuario y clave inicial.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Usuario creado.</response>
    /// <response code="400">Datos inválidos o username/email repetido.</response>
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await users.CreateAsync(request, cancellationToken));

    /// <summary>Edita, activa o desactiva un usuario (no se puede desactivar ni cambiar el rol propio).</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="request">Nuevos datos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Usuario actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">No existe.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken) =>
        Ok(await users.UpdateAsync(id, request, cancellationToken));

    /// <summary>Restablece la clave de un usuario.</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="request">Nueva clave.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Clave restablecida.</response>
    /// <response code="400">Clave inválida.</response>
    /// <response code="404">No existe.</response>
    [HttpPost("{id:guid}/reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await users.ResetPasswordAsync(id, request, cancellationToken);
        return NoContent();
    }
}

/// <summary>Tasas de cambio de referencia (servicio externo protegido con Circuit Breaker).</summary>
[ApiController]
[Route("api/exchange-rates")]
[Tags("Tasas de cambio")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdminOrEmployee)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class ExchangeRatesController(IExchangeRateProvider provider) : ControllerBase
{
    /// <summary>Tasas del dólar (VES, COP, EUR).</summary>
    /// <remarks>
    /// Se consulta un proveedor externo con reintentos, timeout y Circuit Breaker. Si el proveedor no responde
    /// o el circuito está abierto, se devuelve el último valor conocido (Source = "cache").
    /// </remarks>
    /// <response code="200">Tasas.</response>
    /// <response code="503">Proveedor caído y sin valor previo.</response>
    [HttpGet]
    [ProducesResponseType<ExchangeRates>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, "application/problem+json")]
    public async Task<ActionResult<ExchangeRates>> Get(CancellationToken cancellationToken) =>
        Ok(await provider.GetLatestAsync(cancellationToken));
}
