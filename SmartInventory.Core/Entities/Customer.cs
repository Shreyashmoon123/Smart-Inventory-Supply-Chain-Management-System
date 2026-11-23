namespace SmartInventory.Core.Entities;

public class Customer
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}