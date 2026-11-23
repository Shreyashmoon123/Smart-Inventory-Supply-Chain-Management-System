
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.RoleId);

        builder.Property(r => r.RoleName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.RoleName)
            .IsUnique();

        builder.Property(r => r.Description)
            .HasMaxLength(250);

        builder.HasData(
            new Role
            {
                RoleId = 1,
                RoleName = "Admin",
                Description = "Full system access"
            },
            new Role
            {
                RoleId = 2,
                RoleName = "Manager",
                Description = "Manages inventory and operations"
            },
            new Role
            {
                RoleId = 3,
                RoleName = "Staff",
                Description = "Handles daily inventory operations"
            }
        );
    }
}
