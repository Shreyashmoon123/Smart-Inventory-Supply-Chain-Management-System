
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.Payments;

public class CreatePaymentDto
{
    [Range(1, int.MaxValue)]
    public int SalesOrderId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? TransactionReference { get; set; }
}
