using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.HasKey(soi => soi.SalesOrderItemId);

        builder.Property(soi => soi.Quantity).HasPrecision(18, 3);
        builder.Property(soi => soi.UnitPrice).HasPrecision(18, 2);
        builder.Property(soi => soi.TotalPrice).HasPrecision(18, 2);

        builder.HasOne(soi => soi.SalesOrder)
            .WithMany(so => so.Items)
            .HasForeignKey(soi => soi.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(soi => soi.Product)
            .WithMany()
            .HasForeignKey(soi => soi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}