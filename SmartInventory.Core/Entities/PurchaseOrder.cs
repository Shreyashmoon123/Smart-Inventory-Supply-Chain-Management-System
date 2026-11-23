
using SmartInventory.Core.Enums;

namespace SmartInventory.Core.Entities;

public class PurchaseOrder
{
    public int PurchaseOrderId { get; set; }

    public int SupplierId { get; set; }
    public int WarehouseId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
