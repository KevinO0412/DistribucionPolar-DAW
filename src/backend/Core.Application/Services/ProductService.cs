using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Entities;
using Core.Domain.Services;
using Core.Domain.ValueObjects;
using FluentValidation;

namespace Core.Application.Services;

public sealed class ProductService(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator) : IProductService
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(
        Guid? categoryId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(categoryId, search, cancellationToken);
        return products.Select(p => p.ToResponse()).ToList();
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el producto {id}.");

        return product.ToResponse();
    }

    public async Task<ProductResponse> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var normalizedSku = sku.Trim().ToUpperInvariant();
        var product = await productRepository.GetBySkuAsync(normalizedSku, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe un producto con el SKU {normalizedSku}.");

        return product.ToResponse();
    }

    public async Task<IReadOnlyList<ProductResponse>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetLowStockAsync(cancellationToken);
        return products.Select(p => p.ToResponse()).ToList();
    }

    public async Task<StockHealthReport> GetHealthAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el producto {id}.");

        return product.EvaluateHealth();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var category = await GetExistingCategoryAsync(request.CategoryId, cancellationToken);

        var initialStock = new BoxQuantity(request.InitialBoxes, request.InitialLooseUnits);
        var product = new Product(
            request.Name,
            request.Brand,
            await GenerateUniqueSkuAsync(request.Name, category.Name, cancellationToken),
            category.Id,
            request.PriceBox,
            request.PriceUnit,
            request.CostPrice,
            request.UnitsPerBox,
            initialStock.ToUnits(request.UnitsPerBox),
            request.MinStock,
            request.MaxStock,
            request.IsReturnable,
            request.EmptyGroupKey,
            request.IsGiftEligible);

        productRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToResponse(category);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var product = await productRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el producto {id}.");
        var category = await GetExistingCategoryAsync(request.CategoryId, cancellationToken);

        product.UpdateDetails(
            request.Name,
            request.Brand,
            category.Id,
            request.PriceBox,
            request.PriceUnit,
            request.CostPrice,
            request.UnitsPerBox,
            request.MinStock,
            request.MaxStock,
            request.IsReturnable,
            request.EmptyGroupKey,
            request.IsGiftEligible);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToResponse(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el producto {id}.");

        product.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Category> GetExistingCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return await categoryRepository.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new InvalidOperationException($"La categoría {categoryId} no existe.");
    }

    private async Task<string> GenerateUniqueSkuAsync(string name, string categoryName, CancellationToken cancellationToken)
    {
        int sequence = await productRepository.CountIncludingDeletedAsync(cancellationToken) + 1;

        while (true)
        {
            var sku = CorporateSkuGenerator.Generate(name, categoryName, sequence).GeneratedSKU;
            if (!await productRepository.SkuExistsAsync(sku, cancellationToken))
            {
                return sku;
            }

            sequence++;
        }
    }
}
