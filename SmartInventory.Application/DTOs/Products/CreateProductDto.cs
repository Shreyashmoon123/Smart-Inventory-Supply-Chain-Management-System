
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.Products;

public class CreateProductDto
{
    [Required]
    [MaxLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal PurchasePrice { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal SellingPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}
