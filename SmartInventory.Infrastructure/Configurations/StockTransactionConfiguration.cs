using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.HasKey(st => st.StockTransactionId);

        builder.Property(st => st.Quantity)
            .HasPrecision(18, 3);

        builder.Property(st => st.ReferenceType).HasMaxLength(50);
        builder.Property(st => st.Notes).HasMaxLength(500);

        builder.HasOne(st => st.Product)
            .WithMany()
            .HasForeignKey(st => st.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.Warehouse)
            .WithMany()
            .HasForeignKey(st => st.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.User)
            .WithMany()
            .HasForeignKey(st => st.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}