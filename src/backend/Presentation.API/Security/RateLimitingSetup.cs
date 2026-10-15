using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Presentation.API.Middleware;

namespace Presentation.API.Security;

/// <summary>
/// Limitación de tasa (rate limiting) nativa de ASP.NET Core:
/// - "login": ventana fija por IP para frenar ataques de fuerza bruta contra las claves.
/// - Global: ventana fija por usuario autenticado (o por IP si es anónimo) para evitar abusos.
/// Al superar el límite responde 429 en formato RFC 7807 con el encabezado Retry-After.
/// </summary>
public static class RateLimitingSetup
{
    public const string LoginPolicy = "login";

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        int loginLimit = configuration.GetValue("RateLimiting:LoginPermitLimit", 5);
        int globalLimit = configuration.GetValue("RateLimiting:GlobalPermitLimit", 300);

        services.AddRateLimiter(options =>
        {
            options.AddPolicy(LoginPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = loginLimit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.Identity?.IsAuthenticated == true
                    ? $"user:{context.User.Identity.Name}"
                    : $"ip:{context.Connection.RemoteIpAddress}",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = globalLimit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                await ProblemDetailsWriter.WriteAsync(
                    context.HttpContext,
                    StatusCodes.Status429TooManyRequests,
                    "Demasiadas solicitudes",
                    "Se superó el límite de solicitudes permitido. Espere un momento e intente de nuevo.");
            };
        });

        return services;
    }
}
