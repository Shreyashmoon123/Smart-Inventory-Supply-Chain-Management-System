using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.PaymentId);

        builder.Property(p => p.Amount).HasPrecision(18, 2);

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.TransactionReference)
            .HasMaxLength(150);

        builder.HasOne(p => p.SalesOrder)
            .WithMany(so => so.Payments)
            .HasForeignKey(p => p.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.TransactionReference);
    }
}