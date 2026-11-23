using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class StockTransferItemConfiguration : IEntityTypeConfiguration<StockTransferItem>
{
    public void Configure(EntityTypeBuilder<StockTransferItem> builder)
    {
        builder.HasKey(sti => sti.StockTransferItemId);

        builder.Property(sti => sti.Quantity).HasPrecision(18, 3);

        builder.HasOne(sti => sti.StockTransfer)
            .WithMany(st => st.Items)
            .HasForeignKey(sti => sti.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sti => sti.Product)
            .WithMany()
            .HasForeignKey(sti => sti.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}