using System.Text.Json.Serialization;
using Core.Application;
using Core.Application.Abstractions;
using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seed;
using Presentation.API.Middleware;
using Presentation.API.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddApiRateLimiting(builder.Configuration);

// CORS: solo los orígenes configurados por entorno (por ejemplo, el frontend SPA) pueden llamar a la API desde un navegador.
const string CorsPolicy = "frontend";
string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy => policy
    .WithOrigins(allowedOrigins)
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .WithHeaders("Authorization", "Content-Type")
    .WithExposedHeaders("Retry-After", "Content-Disposition")));
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("postgresql");

var app = builder.Build();

// Comportamiento por entorno (appsettings.{Development|Staging|Production}.json o variables de entorno).
bool migrateOnStartup = app.Configuration.GetValue<bool>("Database:MigrateOnStartup");
bool swaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled");

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<DbInitializer>().InitializeAsync(migrateOnStartup);
}

// Primero en el pipeline para capturar las excepciones de todos los componentes siguientes.
app.UseMiddleware<ExceptionMiddleware>();

if (swaggerEnabled)
{
    // Interfaz interactiva en /swagger sobre el documento OpenAPI nativo de .NET 10.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Distribución Polar v1");
        options.DocumentTitle = "Distribución Polar - Swagger";
        options.EnablePersistAuthorization();
    });
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

if (swaggerEnabled)
{
    // Documento OpenAPI 3.1 en JSON (/openapi/v1.json) y YAML (/openapi/v1.yaml).
    app.MapOpenApi().AllowAnonymous();
    app.MapOpenApi("/openapi/{documentName}.yaml").AllowAnonymous();
}

// Sondas para Docker/orquestadores y para verificar el entorno activo.
app.MapHealthChecks("/health").AllowAnonymous().ExcludeFromDescription();
app.MapGet("/api/info", (IWebHostEnvironment environment) => Results.Ok(
        new ApiInfoResponse("Distribución Polar API", environment.EnvironmentName, swaggerEnabled, migrateOnStartup)))
    .AllowAnonymous()
    .WithTags("Sistema")
    .WithSummary("Información del entorno activo")
    .WithDescription("Devuelve el nombre del entorno (Development, Staging o Production) y sus opciones de arranque.")
    .Produces<ApiInfoResponse>();

app.MapGet("/", () => Results.Content(
    """
    <!DOCTYPE html>
    <html lang="es">
    <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <title>Distribución Polar</title>
        <style>
            :root {
                color-scheme: light;
                font-family: Arial, sans-serif;
            }

            body {
                align-items: center;
                background: #f3f6f9;
                color: #17324d;
                display: flex;
                justify-content: center;
                margin: 0;
                min-height: 100vh;
            }

            main {
                background: white;
                border-radius: 16px;
                box-shadow: 0 12px 30px rgba(23, 50, 77, 0.12);
                max-width: 640px;
                padding: 48px;
                text-align: center;
                width: calc(100% - 64px);
            }

            h1 {
                color: #d71920;
                margin-top: 0;
            }

            p {
                line-height: 1.6;
            }
        </style>
    </head>
    <body>
        <main>
            <h1>Distribución Polar</h1>
            <p>La API de Distribución Polar está funcionando correctamente.</p>
            <p>Esta es la página inicial de prueba del sistema.</p>
            <p><a href="/swagger">Abrir Swagger UI</a></p>
        </main>
    </body>
    </html>
    """,
    "text/html; charset=utf-8"))
    .AllowAnonymous()
    .ExcludeFromDescription();

app.MapControllers();

app.Run();

/// <summary>Información del entorno en ejecución.</summary>
/// <param name="Name">Nombre de la API.</param>
/// <param name="Environment">Entorno: Development, Staging o Production.</param>
/// <param name="Swagger">Indica si Swagger/OpenAPI está expuesto.</param>
/// <param name="MigrateOnStartup">Indica si las migraciones se aplican al arrancar.</param>
internal sealed record ApiInfoResponse(string Name, string Environment, bool Swagger, bool MigrateOnStartup);

/// <summary>Expuesto para las pruebas de integración (WebApplicationFactory).</summary>
public partial class Program;
