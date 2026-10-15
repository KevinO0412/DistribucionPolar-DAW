using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Application.Validators;
using Core.Domain.Entities;
using Core.Domain.Services;
using FluentValidation;

namespace Core.Application.Tests;

/// <summary>
/// ProductService en aislamiento total: IProductRepository e ICategoryRepository se simulan con Moq,
/// sin EF Core ni PostgreSQL. Patrón Arrange-Act-Assert.
/// </summary>
public sealed class ProductServiceTests
{
    private readonly Mock<IProductRepository> mockRepo = new();
    private readonly Mock<ICategoryRepository> mockCategories = new();
    private readonly Mock<IUnitOfWork> mockUnitOfWork = TestDoubles.UnitOfWork();
    private readonly ProductService service;
    private readonly Category category = TestDoubles.FoodCategory();

    public ProductServiceTests()
    {
        service = new ProductService(
            mockRepo.Object, mockCategories.Object, mockUnitOfWork.Object,
            new CreateProductRequestValidator(), new UpdateProductRequestValidator());

        mockCategories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
    }

    private CreateProductRequest HarinaPanRequest() =>
        new("HARINA PAN 1KGx20UN", "P.A.N.", category.Id, 18.50m, 0.95m, 14.80m, 20, 2, 5, 100, 800, false, null);

    private UpdateProductRequest UpdateRequest(Guid categoryId) =>
        new("HARINA PAN 1KGx20UN", "P.A.N.", categoryId, 19.00m, 0.98m, 15.20m, 20, 100, 800, false, null);

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProductDto()
    {
        // Arrange
        var product = TestDoubles.HarinaPan(category.Id);
        mockRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var result = await service.GetByIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("HARINA PAN 1KGx20UN", result.Name);
        Assert.Equal("2 cajas + 5 und", result.StockDisplay);
        mockRepo.Verify(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        mockRepo.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(nonExistentId));
        mockRepo.Verify(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_PassesFiltersToRepository_AndMapsEveryProduct()
    {
        // Arrange
        mockRepo.Setup(r => r.GetAllAsync(category.Id, "harina", It.IsAny<CancellationToken>()))
            .ReturnsAsync([TestDoubles.HarinaPan(category.Id), TestDoubles.HarinaPan(category.Id, 0)]);

        // Act
        var result = await service.GetAllAsync(category.Id, "harina");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(0, result[1].StockUnits);
        mockRepo.Verify(r => r.GetAllAsync(category.Id, "harina", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBySkuAsync_NormalizesSkuBeforeQuerying()
    {
        // Arrange
        var product = TestDoubles.HarinaPan(category.Id);
        mockRepo.Setup(r => r.GetBySkuAsync("ALI-HAR-0001", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var result = await service.GetBySkuAsync("  ali-har-0001 ");

        // Assert
        Assert.Equal(product.Id, result.Id);
        mockRepo.Verify(r => r.GetBySkuAsync("ALI-HAR-0001", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBySkuAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        mockRepo.Setup(r => r.GetBySkuAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var error = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetBySkuAsync("xxx-yyy-0001"));
        Assert.Contains("XXX-YYY-0001", error.Message);
    }

    [Fact]
    public async Task GetLowStockAsync_ReturnsProductsFromRepository()
    {
        mockRepo.Setup(r => r.GetLowStockAsync(It.IsAny<CancellationToken>())).ReturnsAsync([TestDoubles.HarinaPan(category.Id, 40)]);

        var result = await service.GetLowStockAsync();

        Assert.Single(result);
        Assert.Equal(40, result[0].StockUnits);
    }

    [Fact]
    public async Task GetHealthAsync_StockUnderMinimum_IsCriticalRisk()
    {
        // Arrange: 45 und con mínimo 100.
        var product = TestDoubles.HarinaPan(category.Id);
        mockRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var report = await service.GetHealthAsync(product.Id);

        // Assert
        Assert.Equal(StockHealthStatus.CriticalRisk, report.Status);
        Assert.Equal(755, report.RecommendedPurchaseUnits);
    }

    [Fact]
    public async Task CreateAsync_GeneratesSkuAndStoresStockInUnits()
    {
        // Arrange
        mockRepo.Setup(r => r.CountIncludingDeletedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        mockRepo.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Product? added = null;
        mockRepo.Setup(r => r.Add(It.IsAny<Product>())).Callback<Product>(p => added = p);

        // Act
        var response = await service.CreateAsync(HarinaPanRequest());

        // Assert
        Assert.Equal("ALI-HAR-0001", response.SKU);
        Assert.Equal(45, response.StockUnits);
        Assert.Equal("Depósito #1 - Alimentos", response.Deposit);
        Assert.NotNull(added);
        Assert.Equal(45, added.StockUnits);
        mockRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SkipsSkuAlreadyTaken()
    {
        // Arrange: la secuencia 0001 ya existe (por ejemplo, un producto eliminado lógicamente).
        mockRepo.Setup(r => r.CountIncludingDeletedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        mockRepo.Setup(r => r.SkuExistsAsync("ALI-HAR-0001", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockRepo.Setup(r => r.SkuExistsAsync("ALI-HAR-0002", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var response = await service.CreateAsync(HarinaPanRequest());

        // Assert
        Assert.Equal("ALI-HAR-0002", response.SKU);
        mockRepo.Verify(r => r.SkuExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateAsync_WithNegativePrice_ThrowsValidation_AndNeverTouchesRepository()
    {
        var request = HarinaPanRequest() with { PriceBox = -1 };

        var error = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.Contains(error.Errors, e => e.PropertyName == nameof(CreateProductRequest.PriceBox));
        mockRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownCategory_ThrowsInvalidOperation()
    {
        var request = HarinaPanRequest() with { CategoryId = Guid.NewGuid() };
        mockCategories.Setup(r => r.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));
        mockRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ChangesPrices_AndSaves()
    {
        // Arrange
        var product = TestDoubles.HarinaPan(category.Id);
        mockRepo.Setup(r => r.GetForUpdateAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var response = await service.UpdateAsync(product.Id, UpdateRequest(category.Id));

        // Assert
        Assert.Equal(19.00m, response.PriceBox);
        Assert.Equal(0.98m, product.PriceUnit);
        Assert.Equal(45, product.StockUnits);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsKeyNotFound_AndDoesNotSave()
    {
        var id = Guid.NewGuid();
        mockRepo.Setup(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(id, UpdateRequest(category.Id)));
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_IsSoftDelete()
    {
        var product = TestDoubles.HarinaPan(category.Id);
        mockRepo.Setup(r => r.GetForUpdateAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await service.DeleteAsync(product.Id);

        Assert.True(product.IsDeleted);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsKeyNotFound()
    {
        var id = Guid.NewGuid();
        mockRepo.Setup(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(id));
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
