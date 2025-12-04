using System.ComponentModel.DataAnnotations;

namespace GamblingApp.Models.DTOs;

/// <summary>
/// Incoming data for adding demo funds to a user's wallet.
/// Used for both the “Add Funds” form and deposit logic.
/// </summary>

public class WalletDepositDto
{
    [Required]
    public int UserId { get; set; }

    [Range(1, 1_000_000)]
    public decimal Amount { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }
}