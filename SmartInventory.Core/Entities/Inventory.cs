namespace SmartInventory.Core.Entities;

public class Inventory
{
    public int InventoryId { get; set; }

    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public decimal Quantity { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public byte[] RowVersion { get; set; } = [];

    public Product Product { get; set; } = null!;

    public Warehouse Warehouse { get; set; } = null!;
}