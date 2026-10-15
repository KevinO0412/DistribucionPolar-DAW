using Core.Application.Abstractions;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seed;

public sealed class SeedUserOptions
{
    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public UserRole Role { get; init; }

    public bool IsActive { get; init; } = true;

    public string? AttendantName { get; init; }

    public bool CanChangeAttendant { get; init; }
}

/// <summary>
/// Inicialización al arrancar: aplica las migraciones pendientes (si el entorno lo permite) y crea
/// los usuarios iniciales configurados en "SeedUsers" cuando la tabla está vacía. Sus claves necesitan
/// hash con sal aleatoria, por eso no van en HasData.
/// </summary>
public sealed class DbInitializer(
    ApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IConfiguration configuration,
    ILogger<DbInitializer> logger)
{
    public const string SeedUsersSection = "SeedUsers";

    public async Task InitializeAsync(bool applyMigrations, CancellationToken cancellationToken = default)
    {
        if (applyMigrations)
        {
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Migraciones aplicadas.");
        }

        var seedUsers = configuration.GetSection(SeedUsersSection).Get<List<SeedUserOptions>>() ?? [];
        if (seedUsers.Count == 0 || await context.Users.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var seed in seedUsers)
        {
            var user = new User(seed.Username, seed.Email, seed.FullName, passwordHasher.Hash(seed.Password), seed.Role,
                seed.AttendantName, seed.CanChangeAttendant);
            if (!seed.IsActive)
            {
                user.Deactivate();
            }

            context.Users.Add(user);
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Usuarios iniciales creados: {Count}.", seedUsers.Count);
    }
}
