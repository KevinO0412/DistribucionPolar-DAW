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
        builder.Property(t => t.DispatchedAt).HasColumnType("timestamptz");
        builder.Ignore(t => t.IsOnRoute);
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

public sealed class RouteSettlementConfiguration : IEntityTypeConfiguration<RouteSettlement>
{
    public void Configure(EntityTypeBuilder<RouteSettlement> builder)
    {
        builder.ToTable("RouteSettlements", table =>
            table.HasCheckConstraint("CK_RouteSettlements_CountedCash_NotNegative", "\"CountedCash\" >= 0"));
        builder.ConfigureBaseEntity();
        builder.Property(s => s.TruckName).IsRequired().HasMaxLength(Truck.NameMaxLength);
        builder.Property(s => s.TruckPlate).IsRequired().HasMaxLength(Truck.PlateMaxLength);
        builder.Property(s => s.DispatchedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(s => s.SettledAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(s => s.SettledBy).HasMaxLength(User.UsernameMaxLength);
        builder.Property(s => s.Observation).HasMaxLength(RouteSettlement.ObservationMaxLength);
        foreach (var money in new[] { nameof(RouteSettlement.SalesTotal), nameof(RouteSettlement.ExpectedCash), nameof(RouteSettlement.CountedCash),
                     nameof(RouteSettlement.CashDifference), nameof(RouteSettlement.MissingGoodsValue) })
        {
            builder.Property<decimal>(money).HasPrecision(18, 2);
        }

        builder.Ignore(s => s.IsBalanced);
        builder.HasIndex(s => new { s.TruckId, s.SettledAt });
        builder.HasOne<Truck>().WithMany().HasForeignKey(s => s.TruckId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(s => s.Lines).WithOne().HasForeignKey(l => l.SettlementId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class RouteSettlementLineConfiguration : IEntityTypeConfiguration<RouteSettlementLine>
{
    public void Configure(EntityTypeBuilder<RouteSettlementLine> builder)
    {
        builder.ToTable("RouteSettlementLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.ProductName).IsRequired().HasMaxLength(Product.NameMaxLength);
        builder.Property(l => l.PriceUnit).HasPrecision(18, 2);
        builder.Property(l => l.DifferenceValue).HasPrecision(18, 2);
        builder.HasOne<Product>().WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
