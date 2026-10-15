using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.API.Security;

namespace Presentation.API.Controllers;

/// <summary>Catálogo de productos. Consulta y registro para Admin y Employee; edición y borrado solo Admin.</summary>
[ApiController]
[Route("api/products")]
[Tags("Productos")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdminOrEmployee)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    /// <summary>Lista los productos del catálogo.</summary>
    /// <remarks>Filtra opcionalmente por categoría y por texto (nombre, marca o SKU, sin distinguir mayúsculas).</remarks>
    /// <param name="categoryId">Filtra por categoría.</param>
    /// <param name="search" example="maltin">Texto a buscar en nombre, marca o SKU.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Productos ordenados por nombre.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        return Ok(await productService.GetAllAsync(categoryId, search, cancellationToken));
    }

    /// <summary>Obtiene un producto por su identificador.</summary>
    /// <param name="id">Identificador UUID del producto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Producto encontrado.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="404">No existe el producto.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await productService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Obtiene un producto por su SKU.</summary>
    /// <param name="sku" example="BEB-MAL-0006">SKU corporativo (no distingue mayúsculas).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Producto encontrado.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="404">No existe un producto con ese SKU.</response>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ProductResponse>> GetBySku(string sku, CancellationToken cancellationToken)
    {
        return Ok(await productService.GetBySkuAsync(sku, cancellationToken));
    }

    /// <summary>Lista los productos con stock en o por debajo del mínimo.</summary>
    /// <response code="200">Productos activos con stock bajo, del menor al mayor stock.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    [HttpGet("low-stock")]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetLowStock(CancellationToken cancellationToken)
    {
        return Ok(await productService.GetLowStockAsync(cancellationToken));
    }

    /// <summary>Diagnostica la salud del inventario de un producto.</summary>
    /// <remarks>Aplica el evaluador de la Fase 1 sobre el stock, el mínimo y el máximo del producto.</remarks>
    /// <param name="id">Identificador UUID del producto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Diagnóstico de salud.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="404">No existe el producto.</response>
    [HttpGet("{id:guid}/health")]
    [ProducesResponseType<StockHealthReport>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<StockHealthReport>> GetHealth(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await productService.GetHealthAsync(id, cancellationToken));
    }

    /// <summary>Registra un producto (Admin y Employee).</summary>
    /// <remarks>
    /// El SKU se genera automáticamente con el formato [CAT]-[PRO]-[SEQ].
    /// El stock inicial se captura en cajas + unidades sueltas y se guarda en unidades.
    /// </remarks>
    /// <param name="request">Datos del producto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Producto creado.</response>
    /// <response code="400">Datos inválidos (precio ≤ 0, stock negativo, máximo ≤ mínimo...) o categoría inexistente.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Edita un producto (solo Admin).</summary>
    /// <remarks>El stock no se modifica con esta operación.</remarks>
    /// <param name="id">Identificador UUID del producto.</param>
    /// <param name="request">Nuevos datos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Producto actualizado.</response>
    /// <response code="400">Datos inválidos o categoría inexistente.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe el producto.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        return Ok(await productService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>Elimina (borrado lógico) un producto (solo Admin).</summary>
    /// <param name="id">Identificador UUID del producto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Producto eliminado.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="403">El rol no es Admin (por ejemplo, Employee).</response>
    /// <response code="404">No existe el producto.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
