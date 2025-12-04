using System.ComponentModel.DataAnnotations;

namespace GamblingApp.Models.DTOs;

/// <summary>
/// Used by admins to credit a user's wallet with extra demo funds.
/// This bypasses normal deposit rules and is only exposed to admin tools.
/// </summary>

public class AdminCreditDto
{
    [Required]
    public int UserId { get; set; }

    [Range(1, 1_000_000)]
    public decimal Amount { get; set; }

    [MaxLength(200)]
    public string? Reason { get; set; }
}