
namespace SmartInventory.Application.DTOs.Payments;

public class PaymentResponseDto
{
    public int PaymentId { get; set; }
    public int SalesOrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public DateTime PaymentDate { get; set; }
}
