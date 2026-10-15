using Core.Application.Abstractions;
using Core.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.API.Security;

namespace Presentation.API.Controllers;

/// <summary>Empleados. Consulta para Admin y Employee; alta y edición solo Admin. No se eliminan: se desactivan.</summary>
[ApiController]
[Route("api/employees")]
[Tags("Empleados")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdminOrEmployee)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class EmployeesController(IEmployeeService employees) : ControllerBase
{
    /// <summary>Lista los empleados ordenados por nombre.</summary>
    /// <param name="includeInactive">Incluye los desactivados (por defecto solo activos).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Empleados.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EmployeeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmployeeResponse>>> GetAll([FromQuery] bool includeInactive, CancellationToken cancellationToken) =>
        Ok(await employees.GetAllAsync(includeInactive, cancellationToken));

    /// <summary>Obtiene un empleado.</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Empleado.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<EmployeeResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await employees.GetByIdAsync(id, cancellationToken));

    /// <summary>Crea un empleado (solo Admin).</summary>
    /// <param name="request">Datos del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Empleado creado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="403">El rol no es Admin.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    public async Task<ActionResult<EmployeeResponse>> Create(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await employees.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    /// <summary>Edita, activa o desactiva un empleado (solo Admin).</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="request">Nuevos datos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Empleado actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<EmployeeResponse>> Update(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken) =>
        Ok(await employees.UpdateAsync(id, request, cancellationToken));
}
