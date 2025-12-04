using System.ComponentModel.DataAnnotations;

namespace GamblingApp.Models.Entities;

/// <summary>
/// Represents a saved payment method for a user.
/// Only used to simulate funding the wallet for demo purposes.
/// Stores cardholder info, masked card number,
/// expiration details, and active status.
/// </summary>

public class PaymentMethod
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }

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