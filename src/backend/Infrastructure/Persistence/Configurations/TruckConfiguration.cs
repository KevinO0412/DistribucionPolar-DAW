using Core.Domain.Entities;
using Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class TruckConfiguration : IEntityTypeConfiguration<Truck>
{
    public void Configure(EntityTypeBuilder<Truck> builder)
    {
        builder.ToTable("Trucks", table =>
            table.HasCheckConstraint("CK_Trucks_CapacityBoxes_Range", $"\"CapacityBoxes\" BETWEEN 1 AND {Truck.MaxCapacityBoxes}"));
        builder.ConfigureBaseEntity();
        builder.UseOptimisticConcurrency();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(Truck.NameMaxLength);
        builder.Property(t => t.Plate).IsRequired().HasMaxLength(Truck.PlateMaxLength);
        builder.Property(t => t.Status).IsRequired().HasMaxLength(Truck.StatusMaxLength);
        builder.Property(t => t.CapacityBoxes).IsRequired().HasDefaultValue(Truck.DefaultCapacityBoxes);
        builder.HasIndex(t => t.Plate).IsUnique().HasFilter("\"IsDeleted\" = false");

        builder.HasMany(t => t.Stock).WithOne().HasForeignKey(s => s.TruckId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.Stock).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(OperationsSeedData.Trucks);
    }
}

public sealed class TruckStockConfiguration : IEntityTypeConfiguration<TruckStock>
{
    public void Configure(EntityTypeBuilder<TruckStock> builder)
    {
        builder.ToTable("TruckStock", table =>
            table.HasCheckConstraint("CK_TruckStock_StockUnits_Positive", "\"StockUnits\" > 0"));
        builder.HasKey(s => new { s.TruckId, s.ProductId });
        builder.Property(s => s.StockUnits).IsRequired();
        builder.HasOne(s => s.Product).WithMany().HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class TruckLoadConfiguration : IEntityTypeConfiguration<TruckLoad>
{
    public void Configure(EntityTypeBuilder<TruckLoad> builder)
    {
        builder.ToTable("TruckLoads");
        builder.ConfigureBaseEntity();
        builder.Property(l => l.TruckName).IsRequired().HasMaxLength(Truck.NameMaxLength);
        builder.Property(l => l.TruckPlate).IsRequired().HasMaxLength(Truck.PlateMaxLength);
        builder.Property(l => l.ProductName).IsRequired().HasMaxLength(Product.NameMaxLength);
        builder.Property(l => l.Source).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(l => l.Observation).HasMaxLength(TruckLoad.ObservationMaxLength);
        builder.Property(l => l.Username).HasMaxLength(User.UsernameMaxLength);
        builder.HasIndex(l => new { l.TruckId, l.CreatedAt });
        builder.HasOne<Truck>().WithMany().HasForeignKey(l => l.TruckId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
