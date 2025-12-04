using GamblingApp.Models.Enums;

namespace GamblingApp.Services
{

    /// <summary>
    /// Calculates the payout multiplier for a slot machine result.
    /// Applies a tiered rule system based on symbol frequency and patterns.
    /// </summary>
    public class SlotPayoutService
    {

         /// <summary>
        /// Determines the payout multiplier for a 3-reel spin.
        /// Jackpot must be triple-Jackpot any other pattern returns 0.
        /// Other tiers (Large, Medium, Small) return scaled multipliers.
        /// </summary>
        public int CalculateMultiplier(SlotSymbol r1, SlotSymbol r2, SlotSymbol r3)
        {
            // Count symbols
            int s = 0, m = 0, l = 0, j = 0;

            SlotSymbol[] reels = { r1, r2, r3 };
            foreach (var sym in reels)
            {
                switch (sym)
                {
                    case SlotSymbol.Small: s++; break;
                    case SlotSymbol.Medium: m++; break;
                    case SlotSymbol.Large: l++; break;
                    case SlotSymbol.Jackpot: j++; break;
                }
            }

            // JACKPOT RULES
            if (j == 3)
                return 50;    // Massive win (JJJ)

            if (j > 0)
                return 0;     // Any J outside JJJ is a fail


            // LARGE TIER
            if (l == 3)
                return 15;    // LLL = Big win

            if (l == 2 && m == 1)
                return 10;    // LLM class = smaller than LLL


            // MEDIUM TIER
            if (m == 3)
                return 7;     // MMM = Medium win

            if (m == 2 && l == 1)
                return 5;     // MML class = smaller than MMM


            // SMALL TIER
            if (s == 3)
                return 3;     // SSS = small win

            if (s == 2 && m == 1)
                return 2;     // SSM class = smaller than SSS


            // ALL OTHER PATTERNS = loss
            return 0;
        }
    }
}