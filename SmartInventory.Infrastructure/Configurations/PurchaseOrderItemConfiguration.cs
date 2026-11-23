using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.HasKey(poi => poi.PurchaseOrderItemId);

        builder.Property(poi => poi.Quantity).HasPrecision(18, 3);
        builder.Property(poi => poi.ReceivedQuantity).HasPrecision(18, 3);
        builder.Property(poi => poi.UnitPrice).HasPrecision(18, 2);
        builder.Property(poi => poi.TotalPrice).HasPrecision(18, 2);

        builder.HasOne(poi => poi.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(poi => poi.Product)
            .WithMany()
            .HasForeignKey(poi => poi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}