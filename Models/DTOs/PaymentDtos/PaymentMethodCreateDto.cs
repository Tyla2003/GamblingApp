using System.ComponentModel.DataAnnotations;

namespace GamblingApp.Models.DTOs;

/// <summary>
/// Incoming model for adding a new payment method to a user's wallet.
/// Used when a user enters card info on the frontend.
/// Basic validation ensures card data looks correct before saving.
/// </summary>

public class PaymentMethodCreateDto
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
}