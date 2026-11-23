
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.PurchaseOrders;

public class CreatePurchaseOrderDto
{
    [Range(1, int.MaxValue)]
    public int SupplierId { get; set; }

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
}
