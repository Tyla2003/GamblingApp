using GamblingApp.Models.DTOs;
using GamblingApp.Models.Entities;
using GamblingApp.Models.Enums;
using GamblingApp.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers;

/// <summary>
/// Handles the simplified Blackjack game flow for the API:
/// validates the bet, deals a fresh hand, scores the result,
/// updates the user's balance, and logs bets and payouts.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class BlackJackController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IGameRepository _gameRepo;
    private readonly IBetRepository _betRepo;
    private readonly ITransactionRepository _transactionRepo;

    /// <summary>
    /// Sets up the Blackjack controller with the repositories it needs
    /// to read users and games, create bets, and log transactions.
    /// </summary>
    public BlackJackController(
        IUserRepository userRepo,
        IGameRepository gameRepo,
        IBetRepository betRepo,
        ITransactionRepository transactionRepo)
    {
        _userRepo = userRepo;
        _gameRepo = gameRepo;
        _betRepo = betRepo;
        _transactionRepo = transactionRepo;
    }

    // --- DTO helpers ---

    /// <summary>
    /// Maps a Bet entity to the read-only DTO that the frontend uses
    /// when showing results from a Blackjack hand.
    /// </summary>
    private static BetReadDto ToBetReadDto(Bet b) => new()
    {
        Id = b.Id,
        UserId = b.UserId,
        GameId = b.GameId,
        BetAmount = b.BetAmount,
        PayoutAmount = b.PayoutAmount,
        Result = b.Result,
        PlacedAt = b.PlacedAt
    };


    // --- Card / hand helpers ---
    /// <summary>
    /// Rank and Suit symbols used in a standard 52-card deck.
    /// </summary>
    private static readonly string[] Ranks = 
        { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

    private static readonly string[] Suits = { "♠", "♥", "♦", "♣" };

    /// <summary>
    /// Creates deck out of cards 
    /// </summary>
    private static List<string> BuildDeck()
    {
        var deck = new List<string>(52);
        foreach (var rank in Ranks)
        {
            foreach (var suit in Suits)
            {
                deck.Add($"{rank}{suit}");
            }
        }
        return deck;
    }

    // Fisher Yates shuffle 
    /// <summary>
    /// Shuffles the deck using Fisher Yates shuffle
    /// </summary>
    private static void Shuffle(List<string> deck)
    {
        var rng = Random.Shared;
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }
    /// <summary>
    /// Returns the Blackjack value for a single card, treating Aces as 11 by default.
    /// Example inputs: "A♠", "10♥", "J♦".
    /// </summary>
    private static int CardValue(string card)
    {
        // card examples: "A♠", "10♥", "J♦"
        if (string.IsNullOrWhiteSpace(card))
            return 0;

        // strip suit (last char) – special case for "10"
        var rankPart = card[..^1];  // everything except last char
        return rankPart switch
        {
            "A" => 11,
            "K" or "Q" or "J" => 10,
            "10" => 10,
            "9" => 9,
            "8" => 8,
            "7" => 7,
            "6" => 6,
            "5" => 5,
            "4" => 4,
            "3" => 3,
            "2" => 2,
            _ => 0
        };
    }

    /// <summary>
    /// Calculates the best Blackjack total for a hand of cards,
    /// automatically downgrading Aces from 11 to 1 while the total is over 21.
    /// </summary>

    private static int HandTotal(List<string> cards)
    {
        int total = 0;
        int aceCount = 0;

        foreach (var c in cards)
        {
            var v = CardValue(c);
            total += v;
            if (c.StartsWith("A"))
            {
                aceCount++;
            }
        }

        // Reduce Aces from 11 to 1 as needed
        while (total > 21 && aceCount > 0)
        {
            total -= 10;   // one Ace becomes 1 instead of 11
            aceCount--;
        }

        return total;
    }


    /// <summary>
    /// Calculates the best Blackjack total for a hand of cards,
    /// automatically downgrading Aces from 11 to 1 while the total is over 21.
    /// </summary>
    private static bool IsBlackjack(List<string> cards)
    {
        if (cards.Count != 2) return false;
        bool hasAce = cards.Any(c => c.StartsWith("A"));
        bool hasTenValue = cards.Any(c =>
        {
            var r = c[..^1];
            return r == "10" || r == "J" || r == "Q" || r == "K";
        });

        return hasAce && hasTenValue;
    }


    /// <summary>
    /// Plays a single simplified Blackjack hand from start to finish:
    /// validates the bet, deals cards, lets the dealer draw to 17,
    /// decides the outcome, updates the balance, and writes bet + transaction records.
    /// </summary>

    // POST: /api/blackjack/play
    [HttpPost("play")]
    public async Task<IActionResult> Play([FromForm] BlackjackPlayRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1) Load user
        var user = await _userRepo.ReadAsync(dto.UserId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        // 2) Load game (Blackjack) + validate
        var game = await _gameRepo.ReadAsync(dto.GameId);
        if (game == null)
        {
            return NotFound(new { message = "Game not found." });
        }

        if (!game.IsEnabled)
        {
            return BadRequest(new { message = "Game is disabled." });
        }

        // Ensures this is really Blackjack
        if (!string.Equals(game.Name, "Blackjack", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Selected game is not Blackjack." });
        }

        var betAmount = dto.BetAmount;

        // 3) Validate bet + balance
        if (betAmount < game.MinBet || betAmount > game.MaxBet)
        {
            return BadRequest(new
            {
                message = $"Bet must be between {game.MinBet} and {game.MaxBet}."
            });
        }

        if (betAmount > user.DemoBalance)
        {
            return BadRequest(new { message = "Insufficient balance for this bet." });
        }

        // 4) Build & shuffle deck, deal cards
        var deck = BuildDeck();
        Shuffle(deck);

        var playerCards = new List<string>();
        var dealerCards = new List<string>();

        // Deal 2 each (player first)
        playerCards.Add(deck[0]);
        dealerCards.Add(deck[1]);
        playerCards.Add(deck[2]);
        dealerCards.Add(deck[3]);

        int playerTotal = HandTotal(playerCards);
        int dealerTotal = HandTotal(dealerCards);

        bool playerBlackjack = IsBlackjack(playerCards);
        bool dealerBlackjack = IsBlackjack(dealerCards);

        string outcome;
        decimal multiplier;

        // 5) Resolve hands (single-step game for simplicity)

        if (playerBlackjack && dealerBlackjack)
        {
            outcome = "Push";
            multiplier = 1.0m; // stake returned
        }
        else if (playerBlackjack)
        {
            outcome = "Win";
            multiplier = 2.5m; // blackjack pays 3:2 equivalent
        }
        else
        {
            // Dealer draws to 17
            while (dealerTotal < 17 && dealerCards.Count < 10)
            {
                var next = deck[playerCards.Count + dealerCards.Count];
                dealerCards.Add(next);
                dealerTotal = HandTotal(dealerCards);
            }

            if (playerTotal > 21)
            {
                outcome = "Lose";
                multiplier = 0m;
            }
            else if (dealerTotal > 21)
            {
                outcome = "Win";
                multiplier = 2.0m; // even money
            }
            else if (playerTotal > dealerTotal)
            {
                outcome = "Win";
                multiplier = 2.0m;
            }
            else if (playerTotal < dealerTotal)
            {
                outcome = "Lose";
                multiplier = 0m;
            }
            else
            {
                outcome = "Push";
                multiplier = 1.0m;
            }
        }

        // 6) Balance + bet / transactions
        user.DemoBalance -= betAmount;

        decimal payoutAmount = betAmount * multiplier;
        if (payoutAmount > 0)
        {
            user.DemoBalance += payoutAmount;
        }

        await _userRepo.UpdateAsync(user.Id, user);

        var bet = new Bet
        {
            UserId = user.Id,
            GameId = game.Id,
            BetAmount = betAmount,
            PayoutAmount = payoutAmount,
            Result = outcome,
            PlacedAt = DateTime.UtcNow
        };

        var createdBet = await _betRepo.CreateAsync(bet);

        // Bet transaction
        var betTx = new Transaction
        {
            UserId = user.Id,
            Amount = betAmount,
            Type = TransactionType.Bet,
            Description = "Blackjack bet",
            BetId = createdBet.Id
        };
        await _transactionRepo.CreateAsync(betTx);

        // Payout transaction (win or push refund)
        if (payoutAmount > 0)
        {
            var payoutTx = new Transaction
            {
                UserId = user.Id,
                Amount = payoutAmount,
                Type = TransactionType.Payout,
                Description = $"Blackjack {outcome}",
                BetId = createdBet.Id
            };
            await _transactionRepo.CreateAsync(payoutTx);
        }

        string message = outcome switch
        {
            "Win" when playerBlackjack => "Blackjack! You win.",
            "Win" => "You win!",
            "Push" => "Push. Your bet is returned.",
            "Lose" => "You lose this hand.",
            _ => "Hand resolved."
        };

        var response = new BlackjackPlayResultDto
        {
            Bet = ToBetReadDto(createdBet),
            NewBalance = user.DemoBalance,
            PlayerHand = playerCards,
            DealerHand = dealerCards,
            PlayerTotal = playerTotal,
            DealerTotal = dealerTotal,
            Outcome = outcome,
            Message = message
        };

        return Ok(response);
    }
}