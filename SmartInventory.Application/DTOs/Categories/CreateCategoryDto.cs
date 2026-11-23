using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.DTOs.Categories;

public class CreateCategoryDto
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}