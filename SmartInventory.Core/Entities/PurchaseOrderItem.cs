
namespace SmartInventory.Core.Entities;

public class PurchaseOrderItem
{
    public int PurchaseOrderItemId { get; set; }

    public int PurchaseOrderId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
