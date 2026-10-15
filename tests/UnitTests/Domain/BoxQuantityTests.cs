using Core.Domain.Exceptions;
using Core.Domain.ValueObjects;

namespace UnitTests.Domain;

public sealed class BoxQuantityTests
{
    [Fact]
    public void ToUnits_ConvertsBoxesAndLooseUnits()
    {
        Assert.Equal(77, new BoxQuantity(2, 5).ToUnits(36));
    }

    [Fact]
    public void FromUnits_SplitsIntoBoxesAndLooseUnits()
    {
        var quantity = BoxQuantity.FromUnits(77, 36);

        Assert.Equal(2, quantity.Boxes);
        Assert.Equal(5, quantity.LooseUnits);
    }

    [Fact]
    public void FromUnits_WithoutUnitsPerBox_KeepsEverythingAsLooseUnits()
    {
        Assert.Equal(new BoxQuantity(0, 10), BoxQuantity.FromUnits(10, 0));
    }

    [Theory]
    [InlineData(2, 5, "2 cajas + 5 und")]
    [InlineData(1, 0, "1 caja")]
    [InlineData(3, 0, "3 cajas")]
    [InlineData(1, 4, "1 caja + 4 und")]
    [InlineData(0, 5, "5 und")]
    public void ToString_UsesSpanishFormat(int boxes, int units, string expected)
    {
        Assert.Equal(expected, new BoxQuantity(boxes, units).ToString());
    }

    [Fact]
    public void Constructor_RejectsNegativeValues()
    {
        Assert.Throws<DomainException>(() => new BoxQuantity(-1, 0));
        Assert.Throws<DomainException>(() => BoxQuantity.FromUnits(-1, 12));
    }
}
