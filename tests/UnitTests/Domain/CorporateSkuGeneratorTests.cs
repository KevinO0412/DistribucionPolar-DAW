using Core.Domain.Services;

namespace UnitTests.Domain;

public sealed class CorporateSkuGeneratorTests
{
    [Fact]
    public void Generate_SanitizesAndFormatsSku()
    {
        var result = CorporateSkuGenerator.Generate("Taladro Percutor 1/2 Pulg 650W Bosch", "Herramientas Eléctricas", 7);

        Assert.Equal("HER-TAL-0007", result.GeneratedSKU);
        Assert.Equal("TALADROPERCUTOR12PULG650WBOSCH", result.SanitizedName);
        Assert.True(result.IsValidForEnterprise);
    }

    [Theory]
    [InlineData("MALTIN POLAR RET 222MLx36UN", "Bebidas", 6, "BEB-MAL-0006")]
    [InlineData("CAFÉ ANZOATEGUI 50X50UN", "Alimentos", 81, "ALI-CAF-0081")]
    [InlineData("ARIEL 1KG", "Jabones/P&G", 200, "JAB-ARI-0200")]
    public void Generate_WithCatalogProducts_ProducesExpectedSku(string name, string category, int sequence, string expected)
    {
        Assert.Equal(expected, CorporateSkuGenerator.Generate(name, category, sequence).GeneratedSKU);
    }

    [Fact]
    public void Generate_PadsShortPrefixesAndClampsSequence()
    {
        Assert.Equal("ABX-ZPP-9999", CorporateSkuGenerator.Generate("z", "ab", 123456).GeneratedSKU);
        Assert.Equal("ABX-ZPP-0001", CorporateSkuGenerator.Generate("z", "ab", -4).GeneratedSKU);
    }

    [Theory]
    [InlineData("", "Bebidas")]
    [InlineData("Maltin", " ")]
    public void Generate_WithMissingInput_Throws(string name, string category)
    {
        Assert.Throws<InvalidOperationException>(() => CorporateSkuGenerator.Generate(name, category, 1));
    }

    [Theory]
    [InlineData("BEB-MAL-0006", true)]
    [InlineData("beb-mal-0006", false)]
    [InlineData("BEB-MAL-06", false)]
    public void IsValid_ChecksCorporateFormat(string sku, bool expected)
    {
        Assert.Equal(expected, CorporateSkuGenerator.IsValid(sku));
    }
}
