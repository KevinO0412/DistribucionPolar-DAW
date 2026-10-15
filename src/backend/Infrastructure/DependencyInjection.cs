using Core.Application.Abstractions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Seed;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");

        // Fábrica de DbContext (la usa la auditoría, aislada del proceso de negocio) + DbContext Scoped por request.
        services.AddDbContextFactory<ApplicationDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(
                maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null))
            // Los registros eliminados lógicamente conservan sus dependientes a propósito (historial).
            .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));
        services.AddScoped(provider => provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Scoped: repositorios y servicios que comparten el DbContext del request.
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ITruckRepository, TruckRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IDamagedProductRepository, DamagedProductRepository>();
        services.AddScoped<IInternalConsumptionRepository, InternalConsumptionRepository>();
        services.AddScoped<IConsignmentRepository, ConsignmentRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IReportQueries, ReportQueries>();
        services.AddScoped<IDocumentNumberGenerator, DocumentNumberGenerator>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IBackupExporter, BackupExporter>();
        services.AddScoped<DbInitializer>();

        // Singleton: servicios sin estado y estado compartido de solo lectura.
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IClock, BusinessClock>();
        services.AddSingleton<IDocumentPdfGenerator, QuestPdfGenerator>();
        services.AddSingleton<ExchangeRateCache>();

        // Transient: una instancia nueva cada vez que se emite un token.
        services.AddTransient<ITokenService, JwtTokenService>();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(s => System.Text.Encoding.UTF8.GetByteCount(s.Key) >= 32, "JwtSettings:Key debe tener al menos 32 bytes.")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer) && !string.IsNullOrWhiteSpace(s.Audience), "JwtSettings:Issuer y Audience son obligatorios.")
            .Validate(s => s.ExpirationMinutes > 0, "JwtSettings:ExpirationMinutes debe ser mayor que 0.")
            .ValidateOnStart();

        // Servicio externo de tasas: timeout por intento, reintentos exponenciales y Circuit Breaker (Polly).
        services.AddOptions<ExchangeRateOptions>().Bind(configuration.GetSection(ExchangeRateOptions.SectionName));
        services.AddHttpClient<IExchangeRateProvider, ExchangeRateProvider>()
            .AddStandardResilienceHandler(resilience =>
            {
                resilience.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
                resilience.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
                resilience.Retry.MaxRetryAttempts = 2;
                resilience.Retry.Delay = TimeSpan.FromMilliseconds(300);
                resilience.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                resilience.CircuitBreaker.MinimumThroughput = 3;
                resilience.CircuitBreaker.FailureRatio = 0.5;
                resilience.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
            });

        return services;
    }
}
