using Core.Application.DTOs;
using Core.Domain.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.API.Controllers;

/// <summary>Endpoints de demostración de la Fase 1 (retos de C# 14). No requieren autenticación.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/demo")]
[Tags("Fase 1 - Demostración")]
[Produces("application/json")]
public sealed class DemoController : ControllerBase
{
    /// <summary>Evalúa la salud de un nivel de stock (reto 1).</summary>
    /// <remarks>Clasifica con pattern matching sobre la tupla (stock, mínimo, máximo).</remarks>
    /// <param name="request">Stock actual, mínimo y máximo.</param>
    /// <param name="validator">Validador de la solicitud.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Diagnóstico de salud.</response>
    /// <response code="400">El máximo no supera al mínimo o hay datos inválidos.</response>
    [HttpPost("csharp/health-calculator")]
    [Consumes("application/json")]
    [ProducesResponseType<StockHealthReport>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<StockHealthReport>> HealthCalculator(
        HealthCalculatorRequest request,
        [FromServices] IValidator<HealthCalculatorRequest> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(ProductHealthEvaluator.Evaluate(request.Sku, request.CurrentStock, request.MinStock, request.MaxStock));
    }

    /// <summary>Genera un SKU corporativo (reto 2).</summary>
    /// <remarks>Sanitiza con expresiones regulares y arma el formato [CAT]-[PRO]-[SEQ] con rangos de C#.</remarks>
    /// <param name="request">Nombre del producto, categoría y secuencia.</param>
    /// <param name="validator">Validador de la solicitud.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">SKU generado.</response>
    /// <response code="400">Datos inválidos.</response>
    [HttpPost("csharp/sku-generator")]
    [Consumes("application/json")]
    [ProducesResponseType<SkuGenerationResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<SkuGenerationResult>> SkuGenerator(
        SkuGeneratorRequest request,
        [FromServices] IValidator<SkuGeneratorRequest> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(CorporateSkuGenerator.Generate(request.RawProductName, request.CategoryName, request.SequenceNumber));
    }

    /// <summary>Simula una excepción no controlada.</summary>
    /// <remarks>Comprueba que el middleware responda 500 en formato RFC 7807 sin exponer el stack trace.</remarks>
    /// <response code="500">Error interno en formato Problem Details.</response>
    [HttpGet("exception")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
    public IActionResult ThrowException()
    {
        throw new Exception("Excepción simulada para probar el middleware.");
    }
}
