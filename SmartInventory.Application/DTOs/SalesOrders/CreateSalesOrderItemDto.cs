
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.SalesOrders;

public class CreateSalesOrderItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; }
}
