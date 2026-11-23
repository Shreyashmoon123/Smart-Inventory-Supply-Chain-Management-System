using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.HasKey(so => so.SalesOrderId);

        builder.Property(so => so.TotalAmount).HasPrecision(18, 2);
        builder.Property(so => so.Notes).HasMaxLength(500);

        builder.Property(so => so.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(so => so.Customer)
            .WithMany(c => c.SalesOrders)
            .HasForeignKey(so => so.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(so => so.OrderDate);
        builder.HasIndex(so => so.Status);
    }
}