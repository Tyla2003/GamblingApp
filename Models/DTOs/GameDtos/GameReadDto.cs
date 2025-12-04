namespace GamblingApp.Models.DTOs;

/// <summary>
/// Read-only view of a game.  
/// Used when sending game info (like Slots / Blackjack) to the client.
/// </summary>

public class GameReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MinBet { get; set; }
    public decimal MaxBet { get; set; }
    public bool IsEnabled { get; set; }
}