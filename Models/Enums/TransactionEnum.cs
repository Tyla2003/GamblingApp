namespace GamblingApp.Models.Enums;

public enum TransactionType
{
    Deposit = 0,     // Adding credits via “card”
    Bet = 1,         // Placing a wager
    Payout = 2,      // Winnings from a bet
    AdminCredit = 3, // Admin added test credits
    Bonus = 4        // Any promo/bonus you may add later
}