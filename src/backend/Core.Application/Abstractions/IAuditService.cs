namespace Core.Application.Abstractions;

/// <summary>Entrada de auditoría. Los objetos Before/After/Metadata se guardan como JSONB.</summary>
public sealed record AuditEntry(
    string Action,
    string Module,
    string Description,
    string? EntityName = null,
    string? EntityId = null,
    decimal? Amount = null,
    object? Before = null,
    object? After = null,
    object? Metadata = null);

/// <summary>
/// Registra la bitácora de auditoría. Se invoca DESPUÉS de confirmar el proceso: si la auditoría falla,
/// el proceso de negocio no se revierte (la falla queda en el log del servidor).
/// </summary>
public interface IAuditService
{
    Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
