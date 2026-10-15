using Core.Domain.Entities;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchases");
        builder.ConfigureBaseEntity();
        builder.Property(p => p.Number).IsRequired().HasMaxLength(Purchase.NumberMaxLength);
        builder.Property(p => p.PurchasedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(p => p.EmployeeName).IsRequired().HasMaxLength(Purchase.EmployeeNameMaxLength);
        builder.Property(p => p.DocumentType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.DocumentNumber).HasMaxLength(Purchase.DocumentNumberMaxLength);
        builder.Property(p => p.TotalAmount).IsRequired().HasPrecision(18, 2);
        builder.HasIndex(p => p.Number).IsUnique();
        builder.HasIndex(p => p.PurchasedAt);
        builder.HasOne<Employee>().WithMany().HasForeignKey(p => p.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(p => p.Items).WithOne().HasForeignKey(i => i.PurchaseId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<PurchaseItem> builder)
    {
        builder.ToTable("PurchaseItems", table =>
            table.HasCheckConstraint("CK_PurchaseItems_TotalUnits_Positive", "\"TotalUnits\" > 0"));
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(Product.NameMaxLength);
        builder.Property(i => i.PriceBox).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.PriceUnit).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.Subtotal).IsRequired().HasPrecision(18, 2);
        builder.HasOne<Product>().WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal static class StockWithdrawalConfiguration
{
    public static void ConfigureWithdrawal<T>(this EntityTypeBuilder<T> builder, string table)
        where T : StockWithdrawal
    {
        builder.ToTable(table, t => t.HasCheckConstraint($"CK_{table}_TotalUnits_Positive", "\"TotalUnits\" > 0"));
        builder.ConfigureBaseEntity();
        builder.Property(w => w.OccurredAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(w => w.Origin).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(w => w.TruckName).HasMaxLength(Truck.NameMaxLength);
        builder.Property(w => w.ProductName).IsRequired().HasMaxLength(Product.NameMaxLength);
        builder.Property(w => w.EstimatedValue).IsRequired().HasPrecision(18, 2);
        builder.Property(w => w.Observation).HasMaxLength(StockWithdrawal.ObservationMaxLength);
        builder.HasIndex(w => w.OccurredAt);
        builder.HasOne<Product>().WithMany().HasForeignKey(w => w.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Truck>().WithMany().HasForeignKey(w => w.TruckId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class DamagedProductConfiguration : IEntityTypeConfiguration<DamagedProduct>
{
    public void Configure(EntityTypeBuilder<DamagedProduct> builder)
    {
        builder.ConfigureWithdrawal("DamagedProducts");
        builder.Property(d => d.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Reason).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.ActionTaken).IsRequired().HasConversion<string>().HasMaxLength(30);
    }
}

public sealed class InternalConsumptionConfiguration : IEntityTypeConfiguration<InternalConsumption>
{
    public void Configure(EntityTypeBuilder<InternalConsumption> builder)
    {
        builder.ConfigureWithdrawal("InternalConsumptions");
        builder.Property(c => c.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.EmployeeName).IsRequired().HasMaxLength(Employee.NameMaxLength);
        builder.HasOne<Employee>().WithMany().HasForeignKey(c => c.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ConsignmentEventConfiguration : IEntityTypeConfiguration<ConsignmentEvent>
{
    public void Configure(EntityTypeBuilder<ConsignmentEvent> builder)
    {
        builder.ToTable("ConsignmentEvents");
        builder.ConfigureBaseEntity();
        builder.UseOptimisticConcurrency();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(ConsignmentEvent.NameMaxLength);
        builder.Property(c => c.Responsible).IsRequired().HasMaxLength(ConsignmentEvent.ResponsibleMaxLength);
        builder.Property(c => c.EventDate).IsRequired().HasColumnType("timestamptz");
        builder.Property(c => c.ClosedAt).HasColumnType("timestamptz");
        builder.HasIndex(c => c.IsClosed);
        builder.HasMany(c => c.Items).WithOne().HasForeignKey(i => i.EventId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ConsignmentItemConfiguration : IEntityTypeConfiguration<ConsignmentItem>
{
    public void Configure(EntityTypeBuilder<ConsignmentItem> builder)
    {
        builder.ToTable("ConsignmentItems", table =>
        {
            table.HasCheckConstraint("CK_ConsignmentItems_Delivered_Positive", "\"DeliveredUnits\" > 0");
            table.HasCheckConstraint("CK_ConsignmentItems_Returned_Valid", "\"ReturnedUnits\" >= 0 AND \"ReturnedUnits\" <= \"DeliveredUnits\"");
        });
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(Product.NameMaxLength);
        builder.Property(i => i.PriceBox).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.PriceUnit).IsRequired().HasPrecision(18, 2);
        builder.HasOne<Product>().WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Bitácora de auditoría: clave BIGINT identity (crecimiento intensivo de solo inserción),
/// estados antes/después en JSONB e IP de origen en el tipo nativo inet.
/// </summary>
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityAlwaysColumn();
        builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Module).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.EntityName).HasMaxLength(100);
        builder.Property(a => a.EntityId).HasMaxLength(50);
        builder.Property(a => a.Amount).HasPrecision(18, 2);
        builder.Property(a => a.BeforeJson).HasColumnType("jsonb");
        builder.Property(a => a.AfterJson).HasColumnType("jsonb");
        builder.Property(a => a.MetadataJson).HasColumnType("jsonb");
        builder.Property(a => a.Username).HasMaxLength(User.UsernameMaxLength);
        builder.Property(a => a.Role).HasMaxLength(20);
        builder.Property(a => a.IpAddress).HasColumnType("inet");
        builder.Property(a => a.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("NOW()");
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => new { a.Module, a.CreatedAt });
    }
}

public sealed class DocumentCounterConfiguration : IEntityTypeConfiguration<DocumentCounter>
{
    public void Configure(EntityTypeBuilder<DocumentCounter> builder)
    {
        builder.ToTable("DocumentCounters");
        builder.HasKey(c => new { c.Sequence, c.Year, c.Month });
        builder.Property(c => c.Sequence).HasMaxLength(30);
        builder.Property(c => c.CurrentNumber).IsRequired();
    }
}
