using System.Text.Json;
using Core.Domain.Services;

namespace Infrastructure.Persistence.Seed;

/// <summary>
/// Datos maestros del catálogo para la siembra declarativa (HasData).
/// Las claves son UUID fijos y deterministas para que las migraciones no detecten cambios ficticios.
/// </summary>
internal static class CatalogSeedData
{
    public static readonly Guid FoodCategoryId = Guid.Parse("a1111111-1111-4111-8111-111111111111");
    public static readonly Guid BeveragesCategoryId = Guid.Parse("b2222222-2222-4222-8222-222222222222");
    public static readonly Guid SoapsCategoryId = Guid.Parse("c3333333-3333-4333-8333-333333333333");

    private static readonly DateTime SeedDate = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly Dictionary<string, (Guid Id, string Deposit, string Description)> CategoriesByName = new()
    {
        ["Alimentos"] = (FoodCategoryId, "Depósito #1 - Alimentos", "Víveres, granos, harinas, café, enlatados y alimentos para mascotas"),
        ["Bebidas"] = (BeveragesCategoryId, "Depósito #2 - Bebidas", "Cervezas, maltas, refrescos y aguas; incluye envases retornables"),
        ["Jabones/P&G"] = (SoapsCategoryId, "Depósito #3 - Jabones/P&G", "Detergentes, limpieza del hogar y cuidado personal")
    };

    public static object[] Categories { get; } = CategoriesByName
        .Select(c => (object)new
        {
            c.Value.Id,
            Name = c.Key,
            c.Value.Description,
            c.Value.Deposit,
            CreatedAt = SeedDate,
            IsDeleted = false
        })
        .ToArray();

    public static object[] Products { get; } = LoadProducts();

    private static object[] LoadProducts()
    {
        using var stream = typeof(CatalogSeedData).Assembly
            .GetManifestResourceStream("Infrastructure.Persistence.Seed.products_catalog.json")
            ?? throw new InvalidOperationException("No se encontró el recurso products_catalog.json.");

        var seeds = JsonSerializer.Deserialize<List<ProductSeed>>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("products_catalog.json está vacío.");

        return seeds.Select(ToSeedRow).ToArray();
    }

    private static object ToSeedRow(ProductSeed seed)
    {
        var category = CategoriesByName[seed.Category];

        // Niveles de inventario y costo de DEMOSTRACIÓN (el catálogo original no los trae):
        // mínimo 5 cajas, máximo 40 cajas, stock variado entre 0 y 44 cajas, costo = 80% del precio por caja.
        int stockBoxes = seed.LegacyId * 7 % 45;

        return new
        {
            Id = Guid.Parse($"00000000-0000-4000-8000-{seed.LegacyId:D12}"),
            seed.Name,
            seed.Brand,
            SKU = CorporateSkuGenerator.Generate(seed.Name, seed.Category, seed.LegacyId).GeneratedSKU,
            CategoryId = category.Id,
            seed.PriceBox,
            seed.PriceUnit,
            CostPrice = Math.Round(seed.PriceBox * 0.80m, 2),
            seed.UnitsPerBox,
            StockUnits = stockBoxes * seed.UnitsPerBox,
            MinStock = 5 * seed.UnitsPerBox,
            MaxStock = 40 * seed.UnitsPerBox,
            seed.IsReturnable,
            seed.EmptyGroupKey,
            seed.IsGiftEligible,
            IsActive = true,
            CreatedAt = SeedDate,
            IsDeleted = false
        };
    }

    private sealed record ProductSeed(
        int LegacyId,
        string Name,
        string Brand,
        string Category,
        decimal PriceBox,
        decimal PriceUnit,
        int UnitsPerBox,
        bool IsReturnable,
        string? EmptyGroupKey,
        bool IsGiftEligible);
}
