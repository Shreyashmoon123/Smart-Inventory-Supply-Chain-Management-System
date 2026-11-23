
namespace SmartInventory.Application.DTOs.Reports;

public class PurchaseReportDto
{
    public int PurchaseOrderId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
