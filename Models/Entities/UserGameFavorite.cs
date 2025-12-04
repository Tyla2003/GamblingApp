namespace GamblingApp.Models.Entities;

/// <summary>
/// Join table for the many-to-many relationship
/// between Users and Games. Each record indicates
/// that a user has marked a game as a favorite.
/// </summary>

public class UserGameFavorite
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public int GameId { get; set; }
    public Game? Game { get; set; }
}