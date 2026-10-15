using Core.Application.Abstractions;
using Core.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.API.Security;

namespace Presentation.API.Controllers;

/// <summary>Categorías del catálogo. Lectura para Admin y Employee; escritura solo Admin.</summary>
[ApiController]
[Route("api/categories")]
[Tags("Categorías")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdminOrEmployee)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    /// <summary>Lista las categorías vigentes.</summary>
    /// <response code="200">Categorías ordenadas por nombre.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await categoryService.GetAllAsync(cancellationToken));
    }

    /// <summary>Obtiene una categoría por su identificador.</summary>
    /// <param name="id">Identificador UUID de la categoría.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Categoría encontrada.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="404">No existe la categoría.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<CategoryResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await categoryService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Crea una categoría (solo Admin).</summary>
    /// <param name="request">Datos de la categoría.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Categoría creada.</response>
    /// <response code="400">Datos inválidos o nombre duplicado.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="403">El rol no es Admin.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await categoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>Edita una categoría (solo Admin).</summary>
    /// <param name="id">Identificador UUID de la categoría.</param>
    /// <param name="request">Nuevos datos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Categoría actualizada.</response>
    /// <response code="400">Datos inválidos o nombre duplicado.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe la categoría.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<CategoryResponse>> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        return Ok(await categoryService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>Elimina (borrado lógico) una categoría sin productos (solo Admin).</summary>
    /// <param name="id">Identificador UUID de la categoría.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Categoría eliminada.</response>
    /// <response code="400">La categoría tiene productos asociados.</response>
    /// <response code="401">Falta el token o no es válido.</response>
    /// <response code="403">El rol no es Admin.</response>
    /// <response code="404">No existe la categoría.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await categoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
