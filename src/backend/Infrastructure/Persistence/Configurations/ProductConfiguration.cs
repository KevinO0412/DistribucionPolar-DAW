using Core.Domain.Entities;
using Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", table =>
        {
            table.HasCheckConstraint("CK_Products_PriceBox_Positive", "\"PriceBox\" > 0");
            table.HasCheckConstraint("CK_Products_PriceUnit_Positive", "\"PriceUnit\" > 0");
            table.HasCheckConstraint("CK_Products_CostPrice_NotNegative", "\"CostPrice\" >= 0");
            table.HasCheckConstraint("CK_Products_UnitsPerBox_Positive", "\"UnitsPerBox\" > 0");
            table.HasCheckConstraint("CK_Products_StockUnits_NotNegative", "\"StockUnits\" >= 0");
            table.HasCheckConstraint("CK_Products_MaxStock_GreaterThan_MinStock", "\"MaxStock\" > \"MinStock\"");
        });
        builder.ConfigureBaseEntity();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(Product.NameMaxLength);
        builder.Property(p => p.Brand).IsRequired().HasMaxLength(Product.BrandMaxLength);
        builder.Property(p => p.SKU).IsRequired().HasMaxLength(Product.SkuMaxLength);
        builder.HasIndex(p => p.SKU).IsUnique(); // Búsqueda puntual por SKU con índice B-Tree.

        // Precisión financiera: numeric(18,2) evita los errores de redondeo de float/double.
        builder.Property(p => p.PriceBox).IsRequired().HasPrecision(18, 2);
        builder.Property(p => p.PriceUnit).IsRequired().HasPrecision(18, 2);
        builder.Property(p => p.CostPrice).IsRequired().HasPrecision(18, 2);

        builder.Property(p => p.UnitsPerBox).IsRequired();
        builder.Property(p => p.StockUnits).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.MinStock).IsRequired();
        builder.Property(p => p.MaxStock).IsRequired();
        builder.Property(p => p.IsReturnable).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.EmptyGroupKey).HasMaxLength(Product.EmptyGroupKeyMaxLength);
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.IsGiftEligible).IsRequired().HasDefaultValue(false);
        builder.UseOptimisticConcurrency();

        builder.HasIndex(p => p.Name);

        // Relación 1:N: no se puede borrar una categoría con productos.
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(CatalogSeedData.Products);
    }
}
