using Microsoft.AspNetCore.Mvc;

namespace Presentation.API.Middleware;

/// <summary>
/// Escribe respuestas de error con el formato RFC 7807 (application/problem+json).
/// </summary>
public static class ProblemDetailsWriter
{
    public static Task WriteAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        ProblemDetails problem = errors is null ? new ProblemDetails() : new ValidationProblemDetails(errors);
        problem.Type = $"https://httpstatuses.com/{statusCode}";
        problem.Title = title;
        problem.Status = statusCode;
        problem.Detail = detail;
        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, contentType: "application/problem+json");
    }
}
