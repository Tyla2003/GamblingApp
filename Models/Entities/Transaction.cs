using System.ComponentModel.DataAnnotations;
using GamblingApp.Models.Enums;
namespace GamblingApp.Models.Entities;

/// <summary>
/// Represents a financial event tied to a user account.
/// Used for deposits, admin credits, bets, and payouts.
/// Stores amount, type, description, timestamp,
/// and optionally links to a Bet when relevant.
/// </summary>

public class Transaction
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }

    [Range(0, 1_000_000)]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? BetId { get; set; }
    public Bet? Bet { get; set; }
}