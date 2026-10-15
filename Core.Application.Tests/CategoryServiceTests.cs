using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Application.Validators;
using Core.Domain.Entities;
using FluentValidation;

namespace Core.Application.Tests;

public sealed class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> mockRepo = new();
    private readonly Mock<IUnitOfWork> mockUnitOfWork = TestDoubles.UnitOfWork();
    private readonly CategoryService service;

    public CategoryServiceTests()
    {
        service = new CategoryService(
            mockRepo.Object, mockUnitOfWork.Object, new CreateCategoryRequestValidator(), new UpdateCategoryRequestValidator());
    }

    [Fact]
    public async Task CreateAsync_WithNewName_AddsAndSaves()
    {
        mockRepo.Setup(r => r.NameExistsAsync("Licores", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var response = await service.CreateAsync(new CreateCategoryRequest("Licores", "Rones y vinos", "Depósito #4 - Licores"));

        Assert.Equal("Licores", response.Name);
        Assert.Equal("Depósito #4 - Licores", response.Deposit);
        mockRepo.Verify(r => r.Add(It.Is<Category>(c => c.Name == "Licores")), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicatedName_IsRejected()
    {
        mockRepo.Setup(r => r.NameExistsAsync("ALIMENTOS", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateCategoryRequest("ALIMENTOS", null, "Depósito #9")));
        mockRepo.Verify(r => r.Add(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithoutDeposit_ThrowsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new CreateCategoryRequest("Licores", null, "")));
        mockRepo.Verify(r => r.NameExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExcludesItselfFromTheUniqueNameCheck()
    {
        var category = TestDoubles.FoodCategory();
        mockRepo.Setup(r => r.GetForUpdateAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        mockRepo.Setup(r => r.NameExistsAsync("Alimentos y Víveres", category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var response = await service.UpdateAsync(category.Id, new UpdateCategoryRequest("Alimentos y Víveres", null, "Depósito #1"));

        Assert.Equal("Alimentos y Víveres", response.Name);
        mockRepo.Verify(r => r.NameExistsAsync("Alimentos y Víveres", category.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsKeyNotFound()
    {
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_WithProducts_IsRejected()
    {
        var category = TestDoubles.FoodCategory();
        mockRepo.Setup(r => r.GetForUpdateAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        mockRepo.Setup(r => r.HasProductsAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(category.Id));

        Assert.False(category.IsDeleted);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithoutProducts_IsSoftDelete()
    {
        var category = TestDoubles.FoodCategory();
        mockRepo.Setup(r => r.GetForUpdateAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        mockRepo.Setup(r => r.HasProductsAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await service.DeleteAsync(category.Id);

        Assert.True(category.IsDeleted);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
