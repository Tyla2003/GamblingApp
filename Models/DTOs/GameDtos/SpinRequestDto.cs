using System.ComponentModel.DataAnnotations;

/// <summary>
/// Incoming request body for a slots spin.  
/// The JS sends this when the user presses the Spin button.
/// </summary>

namespace GamblingApp.Models.DTOs;

public class SpinRequestDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int GameId { get; set; }

    [Range(1, 1_000_000)]
    public decimal BetAmount { get; set; }
}