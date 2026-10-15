using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.API.Security;

namespace Presentation.API.Controllers;

/// <summary>Facturas de venta y regalías. Emitir y consultar: Admin y Employee; anular: solo Admin.</summary>
[ApiController]
[Route("api/invoices")]
[Tags("Facturación")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdminOrEmployee)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class InvoicesController(IInvoiceService invoices) : ControllerBase
{
    /// <summary>Vendedores disponibles para el usuario ("quien atiende").</summary>
    /// <response code="200">Opciones de vendedor.</response>
    [HttpGet("attendants")]
    [ProducesResponseType<AttendantOptionsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AttendantOptionsResponse>> Attendants(CancellationToken cancellationToken) =>
        Ok(await invoices.GetAttendantOptionsAsync(cancellationToken));

    /// <summary>Productos autorizados para regalías.</summary>
    /// <response code="200">Productos autorizados.</response>
    [HttpGet("gift-products")]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GiftProducts(CancellationToken cancellationToken) =>
        Ok(await invoices.GetGiftEligibleProductsAsync(cancellationToken));

    /// <summary>Lista facturas y regalías (proyección paginada).</summary>
    /// <param name="from">Desde (fecha local, inclusive).</param>
    /// <param name="to">Hasta (fecha local, inclusive).</param>
    /// <param name="clientId">Filtra por cliente.</param>
    /// <param name="type">Sale o Gift.</param>
    /// <param name="includeCancelled">Incluye anuladas (por defecto no).</param>
    /// <param name="page">Página (desde 1).</param>
    /// <param name="pageSize">Tamaño de página (1–200).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Página de resultados.</response>
    [HttpGet]
    [ProducesResponseType<PagedResponse<InvoiceSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<InvoiceSummaryResponse>>> List(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] Guid? clientId, [FromQuery] InvoiceType? type,
        [FromQuery] bool includeCancelled = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default) =>
        Ok(await invoices.ListAsync(from, to, clientId, type, includeCancelled, page, pageSize, cancellationToken));

    /// <summary>Obtiene una factura o regalía completa.</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Documento.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<InvoiceResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await invoices.GetByIdAsync(id, cancellationToken));

    /// <summary>Descarga el ticket en PDF.</summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Archivo PDF.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:guid}/ticket")]
    [Produces("application/pdf")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Ticket(Guid id, CancellationToken cancellationToken) =>
        File(await invoices.GetTicketPdfAsync(id, cancellationToken), "application/pdf", $"ticket-{id}.pdf");

    /// <summary>Emite una factura de venta.</summary>
    /// <remarks>
    /// En una sola transacción: verifica el stock de todas las líneas en el origen, descuenta el inventario,
    /// asigna el número mensual ("Factura Octubre 2026 #3"), carga el pendiente (total − abono) al cliente
    /// —un abono mayor al total deja saldo a favor—, registra el abono y los vacíos generados y devueltos por grupo.
    /// </remarks>
    /// <param name="request">Datos de la factura.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Factura emitida.</response>
    /// <response code="400">Datos inválidos, stock insuficiente o vendedor no permitido.</response>
    /// <response code="409">Conflicto de concurrencia; reintentar.</response>
    [HttpPost("sales")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<InvoiceResponse>> CreateSale(CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var invoice = await invoices.CreateSaleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    /// <summary>Emite una regalía (solo productos autorizados; no cobra; sí genera vacíos).</summary>
    /// <param name="request">Datos de la regalía.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Regalía emitida.</response>
    /// <response code="400">Datos inválidos, producto no autorizado o stock insuficiente.</response>
    /// <response code="409">Conflicto de concurrencia; reintentar.</response>
    [HttpPost("gifts")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<InvoiceResponse>> CreateGift(CreateGiftRequest request, CancellationToken cancellationToken)
    {
        var invoice = await invoices.CreateGiftAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    /// <summary>Anula una factura o regalía (solo Admin).</summary>
    /// <remarks>
    /// Devuelve el inventario al origen (galpón o camión), revierte el pendiente de dinero y los vacíos,
    /// y registra el movimiento "Factura anulada". Las anuladas se excluyen de los reportes. No se puede anular dos veces.
    /// </remarks>
    /// <param name="id">Identificador.</param>
    /// <param name="request">Motivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Documento anulado.</response>
    /// <response code="400">Motivo vacío, ya anulado o camión inexistente.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<InvoiceResponse>> Cancel(Guid id, CancelInvoiceRequest request, CancellationToken cancellationToken) =>
        Ok(await invoices.CancelAsync(id, request, cancellationToken));
}
