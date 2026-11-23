
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.PurchaseOrders;

public class ReceivePurchaseOrderDto
{
    [Required]
    [MinLength(1)]
    public List<ReceivePurchaseOrderItemDto> Items { get; set; } = new();

    [MaxLength(500)]
    public string? Notes { get; set; }
}
