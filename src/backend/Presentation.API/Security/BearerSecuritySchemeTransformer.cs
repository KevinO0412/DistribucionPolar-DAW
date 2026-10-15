using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Presentation.API.Security;

/// <summary>
/// Declara el esquema JWT Bearer en el documento OpenAPI para habilitar el botón "Authorize" de Swagger UI.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Title = "Distribución Polar API";
        document.Info.Description = "Fases 1, 2 y 3 (DAW-0423807T). Inicie sesión en POST /api/auth/login y pegue el token en Authorize.";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Token JWT obtenido en /api/auth/login."
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        return Task.CompletedTask;
    }
}
