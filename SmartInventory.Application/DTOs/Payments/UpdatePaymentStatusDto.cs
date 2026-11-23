
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.Payments;

public class UpdatePaymentStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
