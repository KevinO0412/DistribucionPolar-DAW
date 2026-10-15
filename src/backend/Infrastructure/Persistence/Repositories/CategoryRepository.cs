using Core.Application.Abstractions;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<Category?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name.ToLower();
        return context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Name.ToLower() == normalized && c.Id != excludeId, cancellationToken);
    }

    public Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return context.Products.AsNoTracking().AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
    }

    public void Add(Category category)
    {
        context.Categories.Add(category);
    }
}
