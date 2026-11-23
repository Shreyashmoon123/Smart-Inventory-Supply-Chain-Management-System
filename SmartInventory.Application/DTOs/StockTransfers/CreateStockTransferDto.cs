
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.StockTransfers;

public class CreateStockTransferDto
{
    [Range(1, int.MaxValue)]
    public int SourceWarehouseId { get; set; }

    [Range(1, int.MaxValue)]
    public int DestinationWarehouseId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateStockTransferItemDto> Items { get; set; } = new();
}
