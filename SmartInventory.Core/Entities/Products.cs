
namespace SmartInventory.Core.Entities;

public class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public int ReorderLevel { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
