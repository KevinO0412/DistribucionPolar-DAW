using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Domain.Services;
using Core.Domain.ValueObjects;

namespace UnitTests.Domain;

public sealed class ProductTests
{
    private static Product CreatePolarPilsen(int stockUnits = 10) => new(
        name: "POLAR PILSEN RET 222MLx36UN",
        brand: "Polar Pilsen",
        sku: "BEB-POL-0021",
        categoryId: Guid.NewGuid(),
        priceBox: 23.00m,
        priceUnit: 0.64m,
        costPrice: 18.40m,
        unitsPerBox: 36,
        stockUnits: stockUnits,
        minStock: 180,
        maxStock: 1440,
        isReturnable: true,
        emptyGroupKey: EmptyReturnGroups.Ret222MlX36);

    [Fact]
    public void CalculateSubtotal_UsesBoxAndUnitPrices()
    {
        Assert.Equal(49.20m, CreatePolarPilsen().CalculateSubtotal(new BoxQuantity(2, 5)));
    }

    [Fact]
    public void RemoveStock_WhenInsufficient_ThrowsAndKeepsStock()
    {
        var product = CreatePolarPilsen(stockUnits: 10);

        Assert.Throws<DomainException>(() => product.RemoveStock(new BoxQuantity(0, 11)));
        Assert.Equal(10, product.StockUnits);
        Assert.Null(product.LastModifiedAt);
    }

    [Fact]
    public void AddAndRemoveStock_UpdateUnitsAndAuditDate()
    {
        var product = CreatePolarPilsen(stockUnits: 0);

        product.AddStock(new BoxQuantity(3, 5));
        product.RemoveStock(new BoxQuantity(1, 0));

        Assert.Equal(77, product.StockUnits);
        Assert.Equal("2 cajas + 5 und", product.Stock.ToString());
        Assert.NotNull(product.LastModifiedAt);
    }

    [Fact]
    public void Stock_ZeroQuantity_IsRejected()
    {
        var product = CreatePolarPilsen();

        Assert.Throws<DomainException>(() => product.AddStock(new BoxQuantity(0, 0)));
        Assert.Throws<DomainException>(() => product.RemoveStock(new BoxQuantity(0, 0)));
    }

    [Fact]
    public void InventoryValue_IsUnitsTimesUnitPrice()
    {
        Assert.Equal(64.00m, CreatePolarPilsen(stockUnits: 100).InventoryValue);
    }

    [Fact]
    public void EvaluateHealth_UsesProductThresholds()
    {
        Assert.Equal(StockHealthStatus.CriticalRisk, CreatePolarPilsen(stockUnits: 72).EvaluateHealth().Status);
    }

    [Theory]
    [InlineData(0, 0.64, 36, 180, 1440, null)]
    [InlineData(23, 0, 36, 180, 1440, null)]
    [InlineData(23, 0.64, 0, 180, 1440, null)]
    [InlineData(23, 0.64, 36, 180, 180, null)]
    [InlineData(23, 0.64, 36, 180, 1440, "GRUPO_INEXISTENTE")]
    public void Constructor_RejectsInvalidBusinessData(
        decimal priceBox, decimal priceUnit, int unitsPerBox, int minStock, int maxStock, string? emptyGroupKey)
    {
        Assert.Throws<DomainException>(() => new Product(
            "PRODUCTO", "MARCA", "BEB-PRO-0001", Guid.NewGuid(), priceBox, priceUnit, 0m,
            unitsPerBox, 0, minStock, maxStock, false, emptyGroupKey));
    }

    [Fact]
    public void Constructor_RejectsInvalidSku()
    {
        Assert.Throws<DomainException>(() => new Product(
            "PRODUCTO", "MARCA", "SKU-MALO", Guid.NewGuid(), 1m, 1m, 0m, 1, 0, 0, 10, false, null));
    }

    [Fact]
    public void MarkAsDeleted_SetsSoftDeleteFlag()
    {
        var product = CreatePolarPilsen();

        product.MarkAsDeleted();

        Assert.True(product.IsDeleted);
        Assert.NotNull(product.LastModifiedAt);
    }
}
