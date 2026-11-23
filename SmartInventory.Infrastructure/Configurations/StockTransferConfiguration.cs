using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.HasKey(st => st.StockTransferId);

        builder.Property(st => st.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(st => st.Notes).HasMaxLength(500);

        builder.HasOne(st => st.SourceWarehouse)
            .WithMany()
            .HasForeignKey(st => st.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(st => st.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(st => st.TransferDate);
    }
}