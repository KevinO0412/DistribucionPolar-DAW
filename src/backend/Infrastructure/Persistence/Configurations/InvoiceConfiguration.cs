using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices", table =>
        {
            table.HasCheckConstraint("CK_Invoices_Total_NotNegative", "\"Total\" >= 0");
            table.HasCheckConstraint("CK_Invoices_Payment_NotNegative", "\"Payment\" >= 0");
            table.HasCheckConstraint("CK_Invoices_Gift_NoCharge",
                "\"Type\" <> 'Gift' OR (\"Total\" = 0 AND \"Payment\" = 0 AND \"Pending\" = 0)");
            table.HasCheckConstraint("CK_Invoices_Truck_Required", "\"DispatchOrigin\" <> 'Truck' OR \"TruckId\" IS NOT NULL");
        });
        builder.ConfigureBaseEntity();
        builder.UseOptimisticConcurrency();

        builder.Property(i => i.Number).IsRequired().HasMaxLength(Invoice.NumberMaxLength);
        builder.Property(i => i.Type).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(i => i.ClientName).IsRequired().HasMaxLength(Client.NameMaxLength);
        builder.Property(i => i.ClientRif).IsRequired().HasMaxLength(Client.RifMaxLength);
        builder.Property(i => i.IssuedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(i => i.DispatchOrigin).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(i => i.TruckName).HasMaxLength(Truck.NameMaxLength);
        builder.Property(i => i.TruckPlate).HasMaxLength(Truck.PlateMaxLength);
        builder.Property(i => i.AttendantName).IsRequired().HasMaxLength(Invoice.AttendantMaxLength);
        builder.Property(i => i.Total).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.Payment).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.Pending).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.PaymentObservation).HasMaxLength(Invoice.ObservationMaxLength);
        builder.Property(i => i.CancellationReason).HasMaxLength(Invoice.CancellationReasonMaxLength);
        builder.Property(i => i.CancelledAt).HasColumnType("timestamptz");
        builder.Property(i => i.CancelledBy).HasMaxLength(User.UsernameMaxLength);

        builder.HasIndex(i => i.Number).IsUnique();
        builder.HasIndex(i => i.IssuedAt);
        builder.HasIndex(i => new { i.ClientId, i.IssuedAt });

        builder.HasOne<Client>().WithMany().HasForeignKey(i => i.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Truck>().WithMany().HasForeignKey(i => i.TruckId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Employee>().WithMany().HasForeignKey(i => i.EmployeeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Items).WithOne().HasForeignKey(it => it.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(i => i.EmptyGroups).WithOne().HasForeignKey(g => g.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.EmptyGroups).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems", table =>
            table.HasCheckConstraint("CK_InvoiceItems_TotalUnits_Positive", "\"TotalUnits\" > 0"));
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(Product.NameMaxLength);
        builder.Property(i => i.SKU).IsRequired().HasMaxLength(Product.SkuMaxLength);
        builder.Property(i => i.PriceBox).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.PriceUnit).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.Subtotal).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.EmptyGroupKey).HasMaxLength(Product.EmptyGroupKeyMaxLength);
        builder.HasOne<Product>().WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class InvoiceEmptyGroupConfiguration : IEntityTypeConfiguration<InvoiceEmptyGroup>
{
    public void Configure(EntityTypeBuilder<InvoiceEmptyGroup> builder)
    {
        builder.ToTable("InvoiceEmptyGroups");
        builder.HasKey(g => new { g.InvoiceId, g.GroupKey });
        builder.Property(g => g.GroupKey).HasMaxLength(Product.EmptyGroupKeyMaxLength);
    }
}
