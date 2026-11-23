
using SmartInventory.Core.Enums;

namespace SmartInventory.Application.DTOs.PurchaseOrders;

public class PurchaseOrderResponseDto
{
    public int PurchaseOrderId { get; set; }

    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }

    public PurchaseOrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public List<PurchaseOrderItemResponseDto> Items { get; set; } = new();
}

public class PurchaseOrderItemResponseDto
{
    public int PurchaseOrderItemId { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public decimal ReceivedQuantity { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
