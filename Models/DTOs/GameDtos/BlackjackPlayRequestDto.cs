using System.ComponentModel.DataAnnotations;

namespace GamblingApp.Models.DTOs;

/// <summary>
/// Incoming request body for playing a single blackjack hand.  
/// The frontend sends this when the user clicks "Deal" with a bet amount.
/// </summary>

public class BlackjackPlayRequestDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int GameId { get; set; }

    [Range(1, 1_000_000)]
    public decimal BetAmount { get; set; }
}