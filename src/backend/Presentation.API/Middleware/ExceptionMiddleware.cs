using System.Text.Json;
using Core.Application.Exceptions;
using Core.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Presentation.API.Middleware;

/// <summary>
/// Middleware global: transforma cualquier excepción en una respuesta RFC 7807 (Problem Details)
/// y nunca expone el stack trace al cliente.
/// </summary>
public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            await HandleAsync(context, exception);
        }
    }

    private Task HandleAsync(HttpContext context, Exception exception)
    {
        context.Response.Clear();

        switch (exception)
        {
            case ValidationException validation:
                logger.LogInformation("Solicitud con datos inválidos en {Path}.", context.Request.Path);
                var errors = validation.Errors
                    .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
                return ProblemDetailsWriter.WriteAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Datos de entrada inválidos",
                    "Uno o más campos no cumplen las reglas de validación.",
                    errors);

            case RetryLimitExceededException:
                // La base de datos falló de forma transitoria más veces que los reintentos configurados.
                logger.LogError(exception, "Base de datos no disponible tras los reintentos.");
                return ProblemDetailsWriter.WriteAsync(
                    context,
                    StatusCodes.Status503ServiceUnavailable,
                    "Base de datos no disponible",
                    "La base de datos no respondió. Intente de nuevo en unos segundos.");

            case DomainException or InvalidOperationException:
                logger.LogWarning("Regla de negocio violada: {Message}", exception.Message);
                return ProblemDetailsWriter.WriteAsync(
                    context, StatusCodes.Status400BadRequest, "Solicitud inválida", exception.Message);

            case UnauthorizedAccessException:
                logger.LogWarning("Acceso no autorizado: {Message}", exception.Message);
                return ProblemDetailsWriter.WriteAsync(
                    context, StatusCodes.Status401Unauthorized, "No autorizado", exception.Message);

            case KeyNotFoundException:
                logger.LogWarning("Recurso no encontrado: {Message}", exception.Message);
                return ProblemDetailsWriter.WriteAsync(
                    context, StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message);

            case DbUpdateConcurrencyException:
                logger.LogWarning(exception, "Conflicto de concurrencia en {Path}.", context.Request.Path);
                return ProblemDetailsWriter.WriteAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    "Conflicto de concurrencia",
                    "Otra operación modificó los mismos datos al mismo tiempo. Consulte de nuevo e intente otra vez.");

            case ExternalServiceUnavailableException:
                logger.LogWarning(exception, "Servicio externo no disponible.");
                return ProblemDetailsWriter.WriteAsync(
                    context, StatusCodes.Status503ServiceUnavailable, "Servicio externo no disponible", exception.Message);

            case DbUpdateException { InnerException: PostgresException postgres }
                when postgres.SqlState is PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation:
                logger.LogWarning(exception, "Conflicto de integridad en la base de datos ({SqlState}).", postgres.SqlState);
                return ProblemDetailsWriter.WriteAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    "Conflicto de datos",
                    postgres.SqlState == PostgresErrorCodes.UniqueViolation
                        ? "Ya existe un registro con el mismo valor único."
                        : "La operación viola la integridad referencial de los datos.");

            default:
                logger.LogError(exception, "Error interno no controlado.");
                return ProblemDetailsWriter.WriteAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "Error interno del servidor",
                    "Ocurrió un error inesperado al procesar la solicitud.");
        }
    }
}
