
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasKey(po => po.PurchaseOrderId);

        builder.Property(po => po.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(po => po.Notes)
            .HasMaxLength(500);

        builder.Property(po => po.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(po => po.Supplier)
            .WithMany()
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(po => po.Warehouse)
            .WithMany()
            .HasForeignKey(po => po.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(po => po.OrderDate);
        builder.HasIndex(po => po.Status);
    }
}
