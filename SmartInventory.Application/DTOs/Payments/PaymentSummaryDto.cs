
namespace SmartInventory.Application.DTOs.Payments;

public class PaymentSummaryDto
{
    public int SalesOrderId { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public List<PaymentResponseDto> Payments { get; set; } = new();
}
