
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.Suppliers;

public class UpdateSupplierDto
{
    [Required]
    [MaxLength(150)]
    public string SupplierName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; }
}
