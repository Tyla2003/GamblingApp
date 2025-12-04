using System.ComponentModel.DataAnnotations;

namespace GamblingApp.Models.DTOs;

/// <summary>
/// Full response from a resolved blackjack hand.  
/// Includes the saved bet, new balance, both hands, and a simple outcome message.
/// </summary>

public class GameCreateDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    [Range(1, 1_000_000)]
    public decimal MinBet { get; set; }

    [Range(1, 1_000_000)]
    public decimal MaxBet { get; set; }

    public bool IsEnabled { get; set; } = true;
}