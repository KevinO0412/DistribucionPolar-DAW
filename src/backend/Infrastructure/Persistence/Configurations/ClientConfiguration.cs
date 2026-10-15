using Core.Domain.Entities;
using Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.ConfigureBaseEntity();
        builder.UseOptimisticConcurrency();
        builder.Property(c => c.Code).IsRequired();
        builder.Property(c => c.Rif).IsRequired().HasMaxLength(Client.RifMaxLength);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(Client.NameMaxLength);
        builder.Property(c => c.Address).IsRequired().HasMaxLength(Client.AddressMaxLength);
        builder.Property(c => c.Nickname).HasMaxLength(Client.NicknameMaxLength);
        builder.Property(c => c.Type).IsRequired().HasMaxLength(Client.TypeMaxLength);
        builder.Property(c => c.MoneyBalance).IsRequired().HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(c => c.EmptyBoxesBalance).IsRequired().HasDefaultValue(0);
        builder.Property(c => c.EmptyUnitsBalance).IsRequired().HasDefaultValue(0);

        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.Rif).IsUnique().HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(c => c.Name);

        builder.HasMany(c => c.EmptyBalances).WithOne().HasForeignKey(b => b.ClientId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.EmptyBalances).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(OperationsSeedData.Clients);
    }
}

public sealed class ClientEmptyBalanceConfiguration : IEntityTypeConfiguration<ClientEmptyBalance>
{
    public void Configure(EntityTypeBuilder<ClientEmptyBalance> builder)
    {
        builder.ToTable("ClientEmptyBalances");
        builder.HasKey(b => new { b.ClientId, b.GroupKey });
        builder.Property(b => b.GroupKey).HasMaxLength(Product.EmptyGroupKeyMaxLength);
    }
}

public sealed class ClientMovementConfiguration : IEntityTypeConfiguration<ClientMovement>
{
    public void Configure(EntityTypeBuilder<ClientMovement> builder)
    {
        builder.ToTable("ClientMovements");
        builder.ConfigureBaseEntity();
        builder.Property(m => m.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Title).IsRequired().HasMaxLength(ClientMovement.TitleMaxLength);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(ClientMovement.DescriptionMaxLength);
        builder.Property(m => m.Amount).IsRequired().HasPrecision(18, 2);
        builder.HasIndex(m => new { m.ClientId, m.CreatedAt });
        builder.HasIndex(m => new { m.Type, m.CreatedAt });
        builder.HasOne<Client>().WithMany().HasForeignKey(m => m.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Invoice>().WithMany().HasForeignKey(m => m.InvoiceId).OnDelete(DeleteBehavior.Restrict);
    }
}
