using System.Text;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Presentation.API.Middleware;

namespace Presentation.API.Security;

public static class AuthenticationSetup
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "unique_name",
                    RoleClaimType = "role"
                };

                // 401 y 403 también responden con Problem Details (RFC 7807).
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        var detail = context.AuthenticateFailure is SecurityTokenExpiredException
                            ? "El token JWT expiró. Inicie sesión nuevamente."
                            : "Se requiere un token JWT válido en el encabezado Authorization: Bearer <token>.";
                        return ProblemDetailsWriter.WriteAsync(
                            context.HttpContext, StatusCodes.Status401Unauthorized, "No autenticado", detail);
                    },
                    OnForbidden = context => ProblemDetailsWriter.WriteAsync(
                        context.HttpContext,
                        StatusCodes.Status403Forbidden,
                        "Acceso denegado",
                        "Su rol no tiene permiso para realizar esta operación.")
                };
            });

        // Toda ruta exige usuario autenticado salvo las marcadas con [AllowAnonymous].
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }
}
