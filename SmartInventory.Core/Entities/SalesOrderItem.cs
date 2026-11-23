namespace SmartInventory.Core.Entities;

public class SalesOrderItem
{
    public int SalesOrderItemId { get; set; }

    public int SalesOrderId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;

    public Product Product { get; set; } = null!;
}