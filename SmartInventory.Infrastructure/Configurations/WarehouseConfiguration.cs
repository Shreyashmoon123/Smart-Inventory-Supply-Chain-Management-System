using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasKey(w => w.WarehouseId);

        builder.Property(w => w.WarehouseName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(w => w.Location).HasMaxLength(300);
        builder.Property(w => w.ContactNumber).HasMaxLength(20);

        builder.Property(w => w.IsActive).HasDefaultValue(true);
    }
}