
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.Customers;

public class UpdateCustomerDto
{
    [Required]
    [MaxLength(150)]
    public string CustomerName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }
}
