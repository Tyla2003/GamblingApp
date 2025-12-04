namespace GamblingApp.Models.DTOs;


/// <summary>
/// Response for a completed slots spin.  
/// Includes the stored bet, updated balance, and a display string
/// so the UI can show what symbols were rolled.
/// </summary>

public class SpinResultDto
{
    public BetReadDto Bet { get; set; } = new();
    public decimal NewBalance { get; set; }

    // Placeholder for showing UI results like reels, etc.
    public string? DisplayResult { get; set; }
}