using SmartInventory.Core.Enums;

namespace SmartInventory.Core.Entities;

public class SalesOrder
{
    public int SalesOrderId { get; set; }

    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Pending;

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public Customer Customer { get; set; } = null!;

    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}