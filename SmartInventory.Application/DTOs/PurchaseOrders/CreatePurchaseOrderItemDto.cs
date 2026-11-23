
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.PurchaseOrders;

public class CreatePurchaseOrderItemDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999")]
    public decimal Quantity { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal UnitPrice { get; set; }
}
