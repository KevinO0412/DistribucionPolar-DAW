using Core.Application.Abstractions;
using Core.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.API.Security;

namespace Presentation.API.Controllers;

/// <summary>
/// Clientes, saldos y estado de cuenta. Convención: saldo negativo = el cliente debe; positivo = a favor.
/// Operación diaria para Admin y Employee; eliminar y ajustar saldos solo Admin.
/// </summary>
[ApiController]
[Route("api/clients")]
[Tags("Clientes")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdminOrEmployee)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class ClientsController(IClientService clients) : ControllerBase
{
    /// <summary>Busca clientes por nombre, RIF, dirección o referencia.</summary>
    /// <param name="search" example="esquina">Texto a buscar (vacío = todos).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Clientes ordenados por código.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClientResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientResponse>>> Search([FromQuery] string? search, CancellationToken cancellationToken) =>
        Ok(await clients.SearchAsync(search, cancellationToken));

    /// <summary>Cuentas por cobrar: clientes con deuda de dinero o de vacíos.</summary>
    /// <response code="200">Clientes con algún saldo negativo.</response>
    [HttpGet("receivables")]
    [ProducesResponseType<IReadOnlyList<ClientResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientResponse>>> Receivables(CancellationToken cancellationToken) =>
        Ok(await clients.GetReceivablesAsync(cancellationToken));

    /// <summary>Obtiene un cliente con sus saldos.</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Cliente.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ClientResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await clients.GetByIdAsync(id, cancellationToken));

    /// <summary>Estado de cuenta separado por pestañas (facturas, regalías, abonos, vacíos y ajustes).</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Estado de cuenta.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:guid}/statement")]
    [ProducesResponseType<ClientStatementResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ClientStatementResponse>> Statement(Guid id, CancellationToken cancellationToken) =>
        Ok(await clients.GetStatementAsync(id, cancellationToken));

    /// <summary>Crea un cliente (código correlativo automático, tipo "Ocasional", saldos en 0).</summary>
    /// <param name="request">Datos del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Cliente creado.</response>
    /// <response code="400">Datos inválidos o RIF repetido.</response>
    [HttpPost]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<ClientResponse>> Create(CreateClientRequest request, CancellationToken cancellationToken)
    {
        var client = await clients.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    /// <summary>Edita dirección, referencia y tipo. Código, nombre y RIF son inmutables.</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="request">Nuevos datos de contacto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Cliente actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">No existe.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ClientResponse>> Update(Guid id, UpdateClientRequest request, CancellationToken cancellationToken) =>
        Ok(await clients.UpdateAsync(id, request, cancellationToken));

    /// <summary>Elimina (borrado lógico) un cliente sin saldos pendientes (solo Admin).</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Eliminado.</response>
    /// <response code="400">Tiene saldos pendientes.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await clients.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Registra un abono de dinero fuera de factura (suma al saldo).</summary>
    /// <param name="id">Cliente.</param>
    /// <param name="request">Monto y observación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Cliente con el saldo actualizado.</response>
    /// <response code="400">Monto inválido.</response>
    /// <response code="404">No existe.</response>
    [HttpPost("{id:guid}/payments")]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ClientResponse>> Payment(Guid id, ClientPaymentRequest request, CancellationToken cancellationToken) =>
        Ok(await clients.RegisterPaymentAsync(id, request, cancellationToken));

    /// <summary>Registra una devolución de vacíos fuera de factura, por grupo de envase.</summary>
    /// <param name="id">Cliente.</param>
    /// <param name="request">Cajas y unidades devueltas por grupo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Cliente con el saldo de vacíos actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">No existe.</response>
    [HttpPost("{id:guid}/empties-returns")]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ClientResponse>> ReturnEmpties(Guid id, ClientEmptiesReturnRequest request, CancellationToken cancellationToken) =>
        Ok(await clients.ReturnEmptiesAsync(id, request, cancellationToken));

    /// <summary>Ajuste manual de saldos (solo Admin): fija valores absolutos con signo.</summary>
    /// <param name="id">Cliente.</param>
    /// <param name="request">Saldos finales y motivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Cliente con los saldos ajustados.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:guid}/balance-adjustments")]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ClientResponse>> AdjustBalances(Guid id, ClientBalanceAdjustmentRequest request, CancellationToken cancellationToken) =>
        Ok(await clients.AdjustBalancesAsync(id, request, cancellationToken));
}
