using Core.Domain.Entities;

namespace Core.Application.Abstractions;

public interface ICategoryRepository
{
    /// <summary>Lectura sin seguimiento (AsNoTracking).</summary>
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Lectura sin seguimiento (AsNoTracking).</summary>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Entidad con seguimiento de cambios, para actualizar o eliminar.</summary>
    Task<Category?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);

    void Add(Category category);
}
