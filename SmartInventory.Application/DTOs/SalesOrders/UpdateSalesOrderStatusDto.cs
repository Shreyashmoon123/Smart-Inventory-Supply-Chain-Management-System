
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.SalesOrders;

public class UpdateSalesOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
