using System.Net;

namespace Core.Domain.Entities;

/// <summary>
/// Registro de auditoría: quién hizo qué, sobre qué entidad, desde qué IP y con qué datos antes y después.
/// Es de solo inserción (nunca se edita ni se borra). Usa clave BIGINT porque crece de forma intensiva.
/// </summary>
public sealed class AuditLog
{
    // Constructor para EF Core.
    private AuditLog()
    {
    }

    public AuditLog(
        string action,
        string module,
        string description,
        string? entityName,
        string? entityId,
        decimal? amount,
        string? beforeJson,
        string? afterJson,
        string? metadataJson,
        Guid? userId,
        string? username,
        string? role,
        IPAddress? ipAddress,
        DateTime createdAt)
    {
        Action = action;
        Module = module;
        Description = description;
        EntityName = entityName;
        EntityId = entityId;
        Amount = amount;
        BeforeJson = beforeJson;
        AfterJson = afterJson;
        MetadataJson = metadataJson;
        UserId = userId;
        Username = username;
        Role = role;
        IpAddress = ipAddress;
        CreatedAt = createdAt;
    }

    public long Id { get; private set; }

    public string Action { get; private set; } = default!;

    public string Module { get; private set; } = default!;

    public string Description { get; private set; } = default!;

    public string? EntityName { get; private set; }

    public string? EntityId { get; private set; }

    public decimal? Amount { get; private set; }

    public string? BeforeJson { get; private set; }

    public string? AfterJson { get; private set; }

    public string? MetadataJson { get; private set; }

    public Guid? UserId { get; private set; }

    public string? Username { get; private set; }

    public string? Role { get; private set; }

    public IPAddress? IpAddress { get; private set; }

    public DateTime CreatedAt { get; private set; }
}
