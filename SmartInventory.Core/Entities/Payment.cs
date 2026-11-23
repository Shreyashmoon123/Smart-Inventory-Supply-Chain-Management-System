using SmartInventory.Core.Enums;

namespace SmartInventory.Core.Entities;

public class Payment
{
    public int PaymentId { get; set; }

    public int SalesOrderId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? TransactionReference { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public SalesOrder SalesOrder { get; set; } = null!;
}