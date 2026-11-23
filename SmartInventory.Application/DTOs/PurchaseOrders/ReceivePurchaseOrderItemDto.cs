
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.PurchaseOrders;

public class ReceivePurchaseOrderItemDto
{
    [Range(1, int.MaxValue)]
    public int PurchaseOrderItemId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999")]
    public decimal ReceivedQuantity { get; set; }
}
