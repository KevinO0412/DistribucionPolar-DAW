using Microsoft.EntityFrameworkCore;
using Core.Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal static class BaseEntityConfiguration
{
    /// <summary>
    /// Clave UUID, columnas de auditoría en timestamptz y filtro global de borrado lógico.
    /// </summary>
    public static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(e => e.LastModifiedAt).HasColumnType("timestamptz");
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }

    /// <summary>
    /// Concurrencia optimista con la columna de sistema xmin de PostgreSQL: si dos operaciones modifican la
    /// misma fila a la vez, la segunda falla con DbUpdateConcurrencyException (HTTP 409) en lugar de pisar datos.
    /// </summary>
    public static void UseOptimisticConcurrency<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.Property<uint>("Version").IsRowVersion();
    }
}
