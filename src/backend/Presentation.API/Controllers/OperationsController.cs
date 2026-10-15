using Core.Application.Abstractions;
using Core.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.API.Security;

namespace Presentation.API.Controllers;

/// <summary>Entradas de mercancía al galpón (solo Admin).</summary>
[ApiController]
[Route("api/purchases")]
[Tags("Compras")]
[Produces("application/json")]
[Authorize(Roles = Roles.Admin)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class PurchasesController(IPurchaseService purchases) : ControllerBase
{
    /// <summary>Lista las compras.</summary>
    /// <param name="from">Desde (fecha local, inclusive).</param>
    /// <param name="to">Hasta (fecha local, inclusive).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Compras.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PurchaseResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PurchaseResponse>>> List([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken) =>
        Ok(await purchases.ListAsync(from, to, cancellationToken));

    /// <summary>Obtiene una compra.</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Compra.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PurchaseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<PurchaseResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await purchases.GetByIdAsync(id, cancellationToken));

    /// <summary>Registra una entrada de mercancía: suma el stock al galpón y numera "Compra #0001".</summary>
    /// <param name="request">Empleado, documento y productos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Compra registrada.</response>
    /// <response code="400">Datos inválidos o empleado inactivo.</response>
    [HttpPost]
    [ProducesResponseType<PurchaseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<PurchaseResponse>> Create(CreatePurchaseRequest request, CancellationToken cancellationToken)
    {
        var purchase = await purchases.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = purchase.Id }, purchase);
    }
}

/// <summary>Averías y consumos internos (solo Admin): descuentan stock del galpón o de un camión.</summary>
[ApiController]
[Route("api")]
[Tags("Averías y consumos")]
[Produces("application/json")]
[Authorize(Roles = Roles.Admin)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class StockWithdrawalsController(IStockWithdrawalService withdrawals) : ControllerBase
{
    /// <summary>Lista las averías con la pérdida total.</summary>
    /// <param name="from">Desde (fecha local, inclusive).</param>
    /// <param name="to">Hasta (fecha local, inclusive).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Averías y totales.</response>
    [HttpGet("damages")]
    [ProducesResponseType<WithdrawalListResponse<DamageResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<WithdrawalListResponse<DamageResponse>>> ListDamages([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken) =>
        Ok(await withdrawals.ListDamagesAsync(from, to, cancellationToken));

    /// <summary>Registra una avería (pérdida estimada = subtotal a precios actuales).</summary>
    /// <param name="request">Producto, cantidad, origen, tipo, motivo y acción.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Avería registrada.</response>
    /// <response code="400">Datos inválidos o inventario insuficiente.</response>
    [HttpPost("damages")]
    [ProducesResponseType<DamageResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<DamageResponse>> RegisterDamage(CreateDamageRequest request, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await withdrawals.RegisterDamageAsync(request, cancellationToken));

    /// <summary>Lista los consumos internos y regalías a empleados con el valor total.</summary>
    /// <param name="from">Desde (fecha local, inclusive).</param>
    /// <param name="to">Hasta (fecha local, inclusive).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Consumos y totales.</response>
    [HttpGet("internal-consumptions")]
    [ProducesResponseType<WithdrawalListResponse<InternalConsumptionResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<WithdrawalListResponse<InternalConsumptionResponse>>> ListConsumptions([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken) =>
        Ok(await withdrawals.ListConsumptionsAsync(from, to, cancellationToken));

    /// <summary>Registra un consumo interno o regalía a empleado (empleado obligatorio).</summary>
    /// <param name="request">Producto, cantidad, origen, empleado y tipo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Consumo registrado.</response>
    /// <response code="400">Datos inválidos, empleado inactivo o inventario insuficiente.</response>
    [HttpPost("internal-consumptions")]
    [ProducesResponseType<InternalConsumptionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<InternalConsumptionResponse>> RegisterConsumption(CreateInternalConsumptionRequest request, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await withdrawals.RegisterConsumptionAsync(request, cancellationToken));
}

/// <summary>Eventos a consignación (Admin y Employee): se cobra solo lo vendido.</summary>
[ApiController]
[Route("api/consignments")]
[Tags("Consignación")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdminOrEmployee)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class ConsignmentsController(IConsignmentService consignments) : ControllerBase
{
    /// <summary>Lista los eventos.</summary>
    /// <param name="isClosed">Filtra abiertos (false) o cerrados (true).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Eventos.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ConsignmentResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConsignmentResponse>>> List([FromQuery] bool? isClosed, CancellationToken cancellationToken) =>
        Ok(await consignments.ListAsync(isClosed, cancellationToken));

    /// <summary>Indicadores: abiertos, cerrados, por cobrar y unidades.</summary>
    /// <response code="200">Indicadores.</response>
    [HttpGet("indicators")]
    [ProducesResponseType<ConsignmentIndicatorsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsignmentIndicatorsResponse>> Indicators(CancellationToken cancellationToken) =>
        Ok(await consignments.GetIndicatorsAsync(cancellationToken));

    /// <summary>Obtiene un evento.</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Evento.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ConsignmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ConsignmentResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await consignments.GetByIdAsync(id, cancellationToken));

    /// <summary>Descarga la liquidación del evento en PDF.</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Archivo PDF.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Pdf(Guid id, CancellationToken cancellationToken) =>
        File(await consignments.GetPdfAsync(id, cancellationToken), "application/pdf", $"consignacion-{id}.pdf");

    /// <summary>Crea un evento entregando mercancía del galpón (valida y descuenta el stock).</summary>
    /// <param name="request">Nombre, responsable y productos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Evento creado.</response>
    /// <response code="400">Datos inválidos o inventario insuficiente.</response>
    [HttpPost]
    [ProducesResponseType<ConsignmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<ConsignmentResponse>> Create(CreateConsignmentRequest request, CancellationToken cancellationToken)
    {
        var consignment = await consignments.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = consignment.Id }, consignment);
    }

    /// <summary>Registra una devolución (evento abierto; no puede superar lo pendiente) y reintegra el stock.</summary>
    /// <param name="id">Evento.</param>
    /// <param name="request">Producto y cantidad devuelta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Evento actualizado.</response>
    /// <response code="400">Evento cerrado o devolución mayor a lo pendiente.</response>
    /// <response code="404">No existe.</response>
    [HttpPost("{id:guid}/returns")]
    [ProducesResponseType<ConsignmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ConsignmentResponse>> Return(Guid id, ConsignmentReturnRequest request, CancellationToken cancellationToken) =>
        Ok(await consignments.RegisterReturnAsync(id, request, cancellationToken));

    /// <summary>Cierra el evento; ya no admite devoluciones. Total a cobrar = lo vendido.</summary>
    /// <param name="id">Evento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Evento cerrado.</response>
    /// <response code="400">Ya estaba cerrado.</response>
    /// <response code="404">No existe.</response>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType<ConsignmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ConsignmentResponse>> Close(Guid id, CancellationToken cancellationToken) =>
        Ok(await consignments.CloseAsync(id, cancellationToken));
}
