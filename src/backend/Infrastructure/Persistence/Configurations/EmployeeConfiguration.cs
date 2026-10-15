using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Infrastructure.Persistence.Seed;

namespace Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.ConfigureBaseEntity();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(Employee.NameMaxLength);
        builder.Property(e => e.Role).IsRequired().HasMaxLength(Employee.RoleMaxLength);
        builder.Property(e => e.IsActive).IsRequired();
        builder.HasIndex(e => e.Name);
        builder.HasData(OperationsSeedData.Employees);
    }
}
