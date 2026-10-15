using Core.Domain.Entities;

namespace Core.Application.Abstractions;

public interface IProductRepository
{
    /// <summary>Lectura sin seguimiento (AsNoTracking) con la categoría incluida.</summary>
    Task<IReadOnlyList<Product>> GetAllAsync(Guid? categoryId, string? search, CancellationToken cancellationToken = default);

    /// <summary>Lectura sin seguimiento (AsNoTracking) con la categoría incluida.</summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lectura sin seguimiento (AsNoTracking) con la categoría incluida.</summary>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>Productos activos con stock en o por debajo del mínimo (AsNoTracking).</summary>
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default);

    /// <summary>Productos autorizados para regalía (AsNoTracking).</summary>
    Task<IReadOnlyList<Product>> GetGiftEligibleAsync(CancellationToken cancellationToken = default);

    /// <summary>Entidad con seguimiento de cambios, para actualizar o eliminar.</summary>
    Task<Product?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Varias entidades con seguimiento de cambios, indexadas por Id. Incluye productos eliminados lógicamente
    /// para que los reversos (anulaciones, devoluciones) puedan reintegrar su stock.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, Product>> GetManyForUpdateAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>Total de productos, incluidos los eliminados lógicamente (base para la secuencia del SKU).</summary>
    Task<int> CountIncludingDeletedAsync(CancellationToken cancellationToken = default);

    void Add(Product product);
}
