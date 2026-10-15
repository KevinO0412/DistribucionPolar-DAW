using Core.Application.Abstractions;
using Core.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.API.Security;

namespace Presentation.API.Controllers;

/// <summary>Inventario total (galpón + camiones). Consulta para Admin y Employee; ajuste solo Admin.</summary>
[ApiController]
[Route("api/inventory")]
[Tags("Inventario")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdminOrEmployee)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class InventoryController(IInventoryService inventory) : ControllerBase
{
    /// <summary>Resumen de unidades y valor por galpón, camión y categoría.</summary>
    /// <response code="200">Resumen.</response>
    [HttpGet("summary")]
    [ProducesResponseType<InventorySummaryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InventorySummaryResponse>> GetSummary(CancellationToken cancellationToken) =>
        Ok(await inventory.GetSummaryAsync(cancellationToken));

    /// <summary>Indica dónde está un producto: galpón y camiones.</summary>
    /// <param name="productId">Producto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Ubicaciones.</response>
    /// <response code="404">No existe el producto.</response>
    [HttpGet("products/{productId:guid}/locations")]
    [ProducesResponseType<ProductLocationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ProductLocationResponse>> GetLocations(Guid productId, CancellationToken cancellationToken) =>
        Ok(await inventory.GetProductLocationAsync(productId, cancellationToken));

    /// <summary>Ajuste manual del stock del galpón (solo Admin): fija la cantidad absoluta.</summary>
    /// <param name="productId">Producto.</param>
    /// <param name="request">Cantidad final y motivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Producto con su nuevo stock.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe el producto.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("products/{productId:guid}/adjust")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ProductResponse>> Adjust(Guid productId, AdjustWarehouseStockRequest request, CancellationToken cancellationToken) =>
        Ok(await inventory.AdjustWarehouseStockAsync(productId, request, cancellationToken));
}
