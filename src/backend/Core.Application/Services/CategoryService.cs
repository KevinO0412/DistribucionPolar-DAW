using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Entities;
using FluentValidation;

namespace Core.Application.Services;

public sealed class CategoryService(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateCategoryRequest> createValidator,
    IValidator<UpdateCategoryRequest> updateValidator) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(c => c.ToResponse()).ToList();
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la categoría {id}.");

        return category.ToResponse();
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await EnsureUniqueNameAsync(request.Name, null, cancellationToken);

        var category = new Category(request.Name, request.Description, request.Deposit);
        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.ToResponse();
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var category = await categoryRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la categoría {id}.");

        await EnsureUniqueNameAsync(request.Name, id, cancellationToken);

        category.Update(request.Name, request.Description, request.Deposit);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la categoría {id}.");

        if (await categoryRepository.HasProductsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException(
                $"No se puede eliminar la categoría '{category.Name}' porque tiene productos asociados.");
        }

        category.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUniqueNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        if (await categoryRepository.NameExistsAsync(name.Trim(), excludeId, cancellationToken))
        {
            throw new InvalidOperationException($"Ya existe una categoría con el nombre '{name.Trim()}'.");
        }
    }
}
