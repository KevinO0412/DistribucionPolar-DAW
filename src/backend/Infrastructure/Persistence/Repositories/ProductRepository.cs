using Core.Application.Abstractions;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(
        Guid? categoryId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = context.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (categoryId is not null)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{EscapeLikePattern(search.Trim())}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, pattern) ||
                EF.Functions.ILike(p.Brand, pattern) ||
                EF.Functions.ILike(p.SKU, pattern));
        }

        return await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockUnits <= p.MinStock)
            .OrderBy(p => p.StockUnits)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetGiftEligibleAsync(CancellationToken cancellationToken = default)
    {
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.IsGiftEligible)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, Product>> GetManyForUpdateAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var list = ids.Distinct().ToList();
        return await context.Products
            .IgnoreQueryFilters()
            .Where(p => list.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);
    }

    public Task<Product?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        return context.Products.AsNoTracking().IgnoreQueryFilters().AnyAsync(p => p.SKU == sku, cancellationToken);
    }

    public Task<int> CountIncludingDeletedAsync(CancellationToken cancellationToken = default)
    {
        return context.Products.AsNoTracking().IgnoreQueryFilters().CountAsync(cancellationToken);
    }

    public void Add(Product product)
    {
        context.Products.Add(product);
    }

    private static string EscapeLikePattern(string value)
    {
        return value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
    }
}
