using Core.Application.DTOs;
using Core.Application.Validators;

namespace UnitTests.Application;

public sealed class ValidatorTests
{
    private static CreateProductRequest ValidProduct() => new(
        Name: "HARINA PAN 1KGx20UN",
        Brand: "P.A.N.",
        CategoryId: Guid.NewGuid(),
        PriceBox: 18.50m,
        PriceUnit: 0.95m,
        CostPrice: 14.80m,
        UnitsPerBox: 20,
        InitialBoxes: 2,
        InitialLooseUnits: 5,
        MinStock: 100,
        MaxStock: 800,
        IsReturnable: false,
        EmptyGroupKey: null);

    [Fact]
    public void CreateProduct_ValidRequest_Passes()
    {
        Assert.True(new CreateProductRequestValidator().Validate(ValidProduct()).IsValid);
    }

    [Fact]
    public void CreateProduct_InvalidRequest_ReportsEveryRule()
    {
        var request = ValidProduct() with
        {
            Name = "",
            PriceBox = -5,
            PriceUnit = 0,
            UnitsPerBox = 0,
            InitialBoxes = -1,
            MinStock = 100,
            MaxStock = 50,
            EmptyGroupKey = "NO_EXISTE"
        };

        var errors = new CreateProductRequestValidator().Validate(request).Errors.Select(e => e.PropertyName).ToHashSet();

        Assert.Equivalent(
            new[] { "Name", "PriceBox", "PriceUnit", "UnitsPerBox", "InitialBoxes", "MaxStock", "EmptyGroupKey" },
            errors);
    }

    [Fact]
    public void CreateProduct_RejectsMoreThanTwoDecimals()
    {
        Assert.False(new CreateProductRequestValidator().Validate(ValidProduct() with { PriceUnit = 0.6389m }).IsValid);
    }

    [Fact]
    public void UpdateProduct_RequiresMaxGreaterThanMin()
    {
        var request = new UpdateProductRequest("X", "Y", Guid.NewGuid(), 1m, 1m, 0m, 1, 10, 10, false, null);

        Assert.Contains(
            new UpdateProductRequestValidator().Validate(request).Errors,
            e => e.PropertyName == "MaxStock");
    }

    [Theory]
    [InlineData("", "Depósito #4")]
    [InlineData("Licores", "")]
    public void CreateCategory_RequiresNameAndDeposit(string name, string deposit)
    {
        Assert.False(new CreateCategoryRequestValidator().Validate(new CreateCategoryRequest(name, null, deposit)).IsValid);
    }

    [Theory]
    [InlineData("", "Admin2026!")]
    [InlineData("admin", "123")]
    public void Login_RequiresUsernameAndPasswordOfSixChars(string username, string password)
    {
        Assert.False(new LoginRequestValidator().Validate(new LoginRequest(username, password)).IsValid);
    }
}
