using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(s => s.SupplierId);

        builder.Property(s => s.SupplierName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Email).HasMaxLength(150);
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.Address).HasMaxLength(500);

        builder.Property(s => s.IsActive).HasDefaultValue(true);
    }
}