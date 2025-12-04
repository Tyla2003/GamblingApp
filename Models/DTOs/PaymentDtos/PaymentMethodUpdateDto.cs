using System.ComponentModel.DataAnnotations;


/// <summary>
/// Input model for updating an existing payment method.
/// Used in the “Edit Card” form and validated server side.
/// </summary>

namespace GamblingApp.Models.DTOs;

public class PaymentMethodUpdateDto
{
    [Required, MaxLength(100)]
    public string CardholderName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12)]
    public int ExpMonth { get; set; }

    [Range(2024, 2100)]
    public int ExpYear { get; set; }
    public string? Nickname { get; set; } 

    [Required, MaxLength(4)]
    public string Cvv { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}