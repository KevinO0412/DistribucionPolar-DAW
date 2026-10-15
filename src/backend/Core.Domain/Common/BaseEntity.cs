namespace Core.Domain.Common;

/// <summary>
/// Entidad base con identidad UUID y campos de auditoría compartidos por todo el dominio.
/// </summary>
public abstract class BaseEntity
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; }

    public DateTime? LastModifiedAt { get; protected set; }

    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Borrado lógico: el registro se conserva para auditoría y deja de aparecer en las consultas.
    /// </summary>
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        Touch();
    }

    protected void Touch()
    {
        LastModifiedAt = DateTime.UtcNow;
    }
}
