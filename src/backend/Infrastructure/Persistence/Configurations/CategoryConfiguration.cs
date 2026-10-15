using Core.Domain.Entities;
using Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.ConfigureBaseEntity();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(Category.NameMaxLength);
        builder.Property(c => c.Description).HasMaxLength(Category.DescriptionMaxLength);
        builder.Property(c => c.Deposit).IsRequired().HasMaxLength(Category.DepositMaxLength);

        // Nombre único entre las categorías vigentes (índice B-Tree parcial).
        builder.HasIndex(c => c.Name).IsUnique().HasFilter("\"IsDeleted\" = false");

        builder.Navigation(c => c.Products).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(CatalogSeedData.Categories);
    }
}
