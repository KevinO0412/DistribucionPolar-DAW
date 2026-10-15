using Core.Domain.Services;

namespace UnitTests.Domain;

public sealed class ProductHealthEvaluatorTests
{
    [Theory]
    [InlineData(0, StockHealthStatus.OutOfStock, 50)]
    [InlineData(3, StockHealthStatus.CriticalRisk, 47)]
    [InlineData(10, StockHealthStatus.CriticalRisk, 40)]
    [InlineData(30, StockHealthStatus.Understocked, 20)]
    [InlineData(31, StockHealthStatus.Optimal, 0)]
    [InlineData(50, StockHealthStatus.Overstocked, 0)]
    [InlineData(51, StockHealthStatus.Excessive, 0)]
    public void Evaluate_ClassifiesStockByThresholds(int stock, StockHealthStatus expected, int expectedPurchase)
    {
        var report = ProductHealthEvaluator.Evaluate("beb-mal-0006", stock, minStock: 10, maxStock: 50);

        Assert.Equal(expected, report.Status);
        Assert.Equal(expectedPurchase, report.RecommendedPurchaseUnits);
        Assert.Equal("BEB-MAL-0006", report.SKU);
    }

    [Fact]
    public void Evaluate_WithBlankSku_UsesNotAvailable()
    {
        Assert.Equal("N/A", ProductHealthEvaluator.Evaluate("  ", 20, 10, 50).SKU);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(50, 10)]
    public void Evaluate_WhenMaxIsNotGreaterThanMin_Throws(int minStock, int maxStock)
    {
        Assert.Throws<InvalidOperationException>(() => ProductHealthEvaluator.Evaluate("X", 5, minStock, maxStock));
    }
}
