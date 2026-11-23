
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.SalesOrders;

public class CreateSalesOrderDto
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateSalesOrderItemDto> Items { get; set; } = new();
}
