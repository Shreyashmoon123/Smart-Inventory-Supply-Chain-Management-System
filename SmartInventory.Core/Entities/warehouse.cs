
namespace SmartInventory.Core.Entities;

public class Warehouse
{
    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? ContactNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
