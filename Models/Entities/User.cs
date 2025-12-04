using System.ComponentModel.DataAnnotations;

namespace GamblingApp.Models.Entities;

/// <summary>
/// Represents an account in the system.
/// Stores profile info, demo balance, and admin permissions.
/// Acts as the parent entity for transactions, bets,
/// payment methods, and favorite games.
/// </summary>

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    // This is plain-text ONLY for school project.
    [Required, DataType(DataType.Password), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    // Demo balance in fake credits
    [Range(0, 1_000_000)]
    public decimal DemoBalance { get; set; }

    public bool IsAdmin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Bet> Bets { get; set; } = new List<Bet>();
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    public ICollection<UserGameFavorite> FavoriteGames { get; set; } = new List<UserGameFavorite>();
}