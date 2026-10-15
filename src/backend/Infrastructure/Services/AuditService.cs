using System.Text.Json;
using Core.Application.Abstractions;
using Core.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Escribe la bitácora con un DbContext propio (de la fábrica), separado del proceso de negocio:
/// así una falla de auditoría nunca revierte ni bloquea la operación ya confirmada.
/// </summary>
public sealed class AuditService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<AuditService> logger) : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            context.AuditLogs.Add(new AuditLog(
                entry.Action,
                entry.Module,
                entry.Description.Length > 1000 ? entry.Description[..1000] : entry.Description,
                entry.EntityName,
                entry.EntityId,
                entry.Amount,
                Serialize(entry.Before),
                Serialize(entry.After),
                Serialize(entry.Metadata),
                currentUser.UserId,
                currentUser.Username,
                currentUser.Role,
                currentUser.IpAddress,
                timeProvider.GetUtcNow().UtcDateTime));
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo registrar la auditoría {Action} ({Module}).", entry.Action, entry.Module);
        }
    }

    private static string? Serialize(object? value) => value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
}
