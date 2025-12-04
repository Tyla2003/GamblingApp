namespace GamblingApp.Models.DTOs;

/// <summary>
/// Read-only view of a single bet.  
/// Used when we want to send bet info back to the frontend
/// without exposing the full Bet entity.
/// </summary>

public class BetReadDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GameId { get; set; }
    public decimal BetAmount { get; set; }
    public decimal PayoutAmount { get; set; }
    public string Result { get; set; } = string.Empty;
    public DateTime PlacedAt { get; set; }
}