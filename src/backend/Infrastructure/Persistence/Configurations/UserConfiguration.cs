using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.ConfigureBaseEntity();

        builder.Property(u => u.Username).IsRequired().HasMaxLength(User.UsernameMaxLength);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(User.EmailMaxLength);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(User.FullNameMaxLength);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(User.PasswordHashMaxLength);
        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.AttendantName).HasMaxLength(User.AttendantNameMaxLength);
        builder.Property(u => u.CanChangeAttendant).IsRequired().HasDefaultValue(false);

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
