using Core.Application.Abstractions;
using Core.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.API.Security;

namespace Presentation.API.Controllers;

/// <summary>Camiones de reparto y su inventario. Cargar y transferir: Admin y Employee; ajustar, vaciar y administrar: solo Admin.</summary>
[ApiController]
[Route("api/trucks")]
[Tags("Camiones")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdminOrEmployee)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class TrucksController(ITruckService trucks) : ControllerBase
{
    /// <summary>Lista los camiones con su inventario y valor.</summary>
    /// <response code="200">Camiones.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TruckResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TruckResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await trucks.GetAllAsync(cancellationToken));

    /// <summary>Obtiene un camión con su inventario.</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Camión.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TruckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TruckResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await trucks.GetByIdAsync(id, cancellationToken));

    /// <summary>Crea un camión (solo Admin).</summary>
    /// <param name="request">Nombre y placa.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Camión creado.</response>
    /// <response code="400">Datos inválidos o placa repetida.</response>
    /// <response code="403">El rol no es Admin.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType<TruckResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    public async Task<ActionResult<TruckResponse>> Create(TruckRequest request, CancellationToken cancellationToken)
    {
        var truck = await trucks.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = truck.Id }, truck);
    }

    /// <summary>Edita nombre y placa (solo Admin).</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="request">Nombre y placa.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Camión actualizado.</response>
    /// <response code="400">Datos inválidos o placa repetida.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TruckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TruckResponse>> Update(Guid id, TruckRequest request, CancellationToken cancellationToken) =>
        Ok(await trucks.UpdateAsync(id, request, cancellationToken));

    /// <summary>Carga el camión desde el galpón.</summary>
    /// <remarks>
    /// Modo Add: agrega la cantidad. Modo Target: la cantidad es la meta final; se carga la diferencia
    /// (si el camión ya tiene esa cantidad o más, 400). Valida el stock del galpón y registra el historial.
    /// </remarks>
    /// <param name="id">Camión.</param>
    /// <param name="request">Producto, modo y cantidad.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Camión actualizado.</response>
    /// <response code="400">Datos inválidos, stock insuficiente o meta ya alcanzada.</response>
    /// <response code="404">No existe el camión.</response>
    [HttpPost("{id:guid}/load")]
    [ProducesResponseType<TruckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TruckResponse>> Load(Guid id, LoadTruckRequest request, CancellationToken cancellationToken) =>
        Ok(await trucks.LoadAsync(id, request, cancellationToken));

    /// <summary>Transfiere (desmonta) mercancía del camión al galpón o a otro camión.</summary>
    /// <param name="id">Camión de origen.</param>
    /// <param name="request">Producto, cantidad y destino.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Camión de origen actualizado.</response>
    /// <response code="400">Datos inválidos o stock insuficiente en el camión.</response>
    /// <response code="404">No existe el camión.</response>
    [HttpPost("{id:guid}/transfer")]
    [ProducesResponseType<TruckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TruckResponse>> Transfer(Guid id, TransferFromTruckRequest request, CancellationToken cancellationToken) =>
        Ok(await trucks.TransferAsync(id, request, cancellationToken));

    /// <summary>Ajuste manual de la cantidad de un producto en el camión (solo Admin).</summary>
    /// <param name="id">Camión.</param>
    /// <param name="request">Producto, cantidad final y motivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Camión actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe el camión.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:guid}/adjust")]
    [ProducesResponseType<TruckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TruckResponse>> Adjust(Guid id, AdjustStockRequest request, CancellationToken cancellationToken) =>
        Ok(await trucks.AdjustAsync(id, request, cancellationToken));

    /// <summary>Vacía el camión devolviendo toda su mercancía al galpón (solo Admin).</summary>
    /// <param name="id">Camión.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Camión vacío.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe el camión.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:guid}/unload")]
    [ProducesResponseType<TruckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TruckResponse>> Unload(Guid id, CancellationToken cancellationToken) =>
        Ok(await trucks.UnloadAsync(id, cancellationToken));

    /// <summary>Historial de cargas de camiones (máximo 500, de la más reciente a la más antigua).</summary>
    /// <param name="truckId">Filtra por camión.</param>
    /// <param name="from">Desde (fecha local, inclusive).</param>
    /// <param name="to">Hasta (fecha local, inclusive).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Cargas.</response>
    [HttpGet("loads")]
    [ProducesResponseType<IReadOnlyList<TruckLoadResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TruckLoadResponse>>> GetLoads(
        [FromQuery] Guid? truckId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken) =>
        Ok(await trucks.GetLoadsAsync(truckId, from, to, cancellationToken));
}
