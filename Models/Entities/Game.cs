using System.ComponentModel.DataAnnotations;

namespace GamblingApp.Models.Entities;

/// <summary>
/// Defines a casino game such as Slots or Blackjack.
/// Stores basic game info like name, description,
/// min/max bet limits, and whether the game is available.
/// Also participates in the many-to-many FavoriteGames relationship.
/// </summary>

public class Game
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty; 

    [MaxLength(200)]
    public string? Description { get; set; }

    [Range(1, 1_000_000)]
    public decimal MinBet { get; set; } = 1;

    [Range(1, 1_000_000)]
    public decimal MaxBet { get; set; } = 1_000;

    public bool IsEnabled { get; set; } = true;

    public ICollection<UserGameFavorite> FavoritedBy { get; set; } = new List<UserGameFavorite>();
}