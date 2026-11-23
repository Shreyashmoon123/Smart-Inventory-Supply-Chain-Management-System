
namespace SmartInventory.Application.DTOs.Reports;

public class SalesReportDto
{
    public int SalesOrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
