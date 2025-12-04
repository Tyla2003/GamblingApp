namespace GamblingApp.Models.DTOs;

/// <summary>
/// Full response from a resolved blackjack hand.  
/// Includes the saved bet, new balance, both hands, and a simple outcome message.
/// </summary>

public class BlackjackPlayResultDto
{
    public BetReadDto Bet { get; set; } = default!;

    public decimal NewBalance { get; set; }

    public List<string> PlayerHand { get; set; } = new();
    public List<string> DealerHand { get; set; } = new();

    public int PlayerTotal { get; set; }
    public int DealerTotal { get; set; }

    // "Win" | "Lose" | "Push"
    public string Outcome { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}