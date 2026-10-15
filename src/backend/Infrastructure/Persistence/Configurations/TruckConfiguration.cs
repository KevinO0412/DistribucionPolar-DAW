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
        {
            table.HasCheckConstraint("CK_Trucks_CapacityBoxes_Range", $"\"CapacityBoxes\" BETWEEN 1 AND {Truck.MaxCapacityBoxes}");
            table.HasCheckConstraint("CK_Trucks_Status",
                $"\"Status\" IN ('{Truck.StatusInWarehouse}', '{Truck.StatusLoading}', '{Truck.StatusOnRoute}', '{Truck.StatusSettled}')");
        });
        builder.ConfigureBaseEntity();
        builder.UseOptimisticConcurrency();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(Truck.NameMaxLength);
        builder.Property(t => t.Plate).IsRequired().HasMaxLength(Truck.PlateMaxLength);
        builder.Property(t => t.Status).IsRequired().HasMaxLength(Truck.StatusMaxLength);
        builder.Property(t => t.DispatchedAt).HasColumnType("timestamptz");
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

public sealed class TruckSettlementConfiguration : IEntityTypeConfiguration<TruckSettlement>
{
    public void Configure(EntityTypeBuilder<TruckSettlement> builder)
    {
        builder.ToTable("TruckSettlements", table =>
            table.HasCheckConstraint("CK_TruckSettlements_CountedCash_NotNegative", "\"CountedCash\" >= 0"));
        builder.ConfigureBaseEntity();
        builder.Property(s => s.TruckName).IsRequired().HasMaxLength(Truck.NameMaxLength);
        builder.Property(s => s.TruckPlate).IsRequired().HasMaxLength(Truck.PlateMaxLength);
        builder.Property(s => s.TripStartedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(s => s.SettledAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(s => s.SettledBy).HasMaxLength(User.UsernameMaxLength);
        builder.Property(s => s.Observation).HasMaxLength(TruckSettlement.ObservationMaxLength);
        foreach (var money in new[] { nameof(TruckSettlement.ExpectedCash), nameof(TruckSettlement.CountedCash), nameof(TruckSettlement.CashDifference),
                     nameof(TruckSettlement.ExpectedMerchandiseValue), nameof(TruckSettlement.CountedMerchandiseValue), nameof(TruckSettlement.MerchandiseDifferenceValue) })
        {
            builder.Property<decimal>(money).IsRequired().HasPrecision(18, 2);
        }

        builder.Ignore(s => s.IsBalanced);
        builder.HasIndex(s => new { s.TruckId, s.SettledAt });
        builder.HasOne<Truck>().WithMany().HasForeignKey(s => s.TruckId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(s => s.Lines).WithOne().HasForeignKey(l => l.SettlementId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class TruckSettlementLineConfiguration : IEntityTypeConfiguration<TruckSettlementLine>
{
    public void Configure(EntityTypeBuilder<TruckSettlementLine> builder)
    {
        builder.ToTable("TruckSettlementLines", table =>
            table.HasCheckConstraint("CK_TruckSettlementLines_Units_NotNegative", "\"ExpectedUnits\" >= 0 AND \"CountedUnits\" >= 0"));
        builder.HasKey(l => new { l.SettlementId, l.ProductId });
        builder.Property(l => l.ProductName).IsRequired().HasMaxLength(Product.NameMaxLength);
        builder.Property(l => l.SKU).IsRequired().HasMaxLength(Product.SkuMaxLength);
        builder.Property(l => l.PriceUnit).IsRequired().HasPrecision(18, 2);
        builder.Ignore(l => l.DifferenceUnits);
        builder.Ignore(l => l.ExpectedValue);
        builder.Ignore(l => l.CountedValue);
        builder.Ignore(l => l.DifferenceValue);
        builder.HasOne<Product>().WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
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
