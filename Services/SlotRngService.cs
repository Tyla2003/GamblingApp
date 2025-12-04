using GamblingApp.Models.Enums;

namespace GamblingApp.Services;

    /// <summary>
    /// Provides randomness for the slot machine reels.
    /// Uses weighted probabilities to generate a symbol.
    /// </summary>

public class SlotRngService
{
        /// <summary>
        /// Returns a random symbol using weighted probability:
        /// Small 60%, Medium 25%, Large 10%, Jackpot 5%.
        /// </summary>
    public SlotSymbol GetRandomSymbol()
    {
        // 1–100 inclusive
        int roll = Random.Shared.Next(1, 101);

        if (roll <= 60)
            return SlotSymbol.Small;

        if (roll <= 85)
            return SlotSymbol.Medium;

        if (roll <= 95)
            return SlotSymbol.Large;

        return SlotSymbol.Jackpot;
    }


        /// <summary>
        /// Spins all three reels and returns the three results as a tuple.
        /// </summary>
    public (SlotSymbol Reel1, SlotSymbol Reel2, SlotSymbol Reel3) SpinThree()
    {
        var r1 = GetRandomSymbol();
        var r2 = GetRandomSymbol();
        var r3 = GetRandomSymbol();

        return (r1, r2, r3);
    }
}