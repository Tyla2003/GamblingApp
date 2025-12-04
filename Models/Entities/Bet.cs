using System.ComponentModel.DataAnnotations;
using GamblingApp.Controllers;

namespace GamblingApp.Models.Entities;

/// <summary>
/// Represents a single wager placed by a user on a game.
/// Stores bet amount, payout amount, result, and timestamp.
/// Each bet belongs to one User and one Game and may have
/// multiple Transaction records linked to it (bet + payout).
/// </summary>

public class Bet
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }
    [Required]
    public int GameId { get; set; }
    public Game? Game { get; set; }

    [Required, MaxLength(30)]
    public string GameType { get; set; } = "Slots";

    [Range(1, 1_000_000)]
    public decimal BetAmount { get; set; }

    [Range(0, 1_000_000)]
    public decimal PayoutAmount { get; set; }

    // "Win" / "Lose" / maybe "Push"
    [Required, MaxLength(10)]
    public string Result { get; set; } = "Lose";

    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;

    // Navigation: transactions tied to this bet (bet + payout)
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}