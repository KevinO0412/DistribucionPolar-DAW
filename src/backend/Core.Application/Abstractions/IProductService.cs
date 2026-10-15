using Core.Application.DTOs;
using Core.Domain.Services;

namespace Core.Application.Abstractions;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(Guid? categoryId, string? search, CancellationToken cancellationToken = default);

    Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductResponse> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductResponse>> GetLowStockAsync(CancellationToken cancellationToken = default);

    Task<StockHealthReport> GetHealthAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
