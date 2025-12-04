using GamblingApp.Models.DTOs;
using GamblingApp.Models.Entities;
using GamblingApp.Models.Enums;
using GamblingApp.Services;
using GamblingApp.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers;

/// <summary>
/// Handles the server-side logic for the Slots game.
/// This controller:
/// - Validates the player and game
/// - Checks bet limits and balance
/// - Uses SlotRngService to spin the reels
/// - Uses SlotPayoutService to calculate winnings
/// - Updates the user balance, creates Bet and Transaction records
/// - Returns a simple DTO back to the JavaScript front end
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class SlotsController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IGameRepository _gameRepo;
    private readonly IBetRepository _betRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly SlotRngService _rng;
    private readonly SlotPayoutService _payout;

    public SlotsController(
        IUserRepository userRepo,
        IGameRepository gameRepo,
        IBetRepository betRepo,
        ITransactionRepository transactionRepo,
        SlotRngService rng,
        SlotPayoutService payout)
    {
        _userRepo = userRepo;
        _gameRepo = gameRepo;
        _betRepo = betRepo;
        _transactionRepo = transactionRepo;
        _rng = rng;
        _payout = payout;
    }

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

    /// <summary>
    /// Converts a SlotSymbol enum into a short display string used in the UI.
    /// </summary>

    private static string SymbolToString(SlotSymbol sym) => sym switch
    {
        SlotSymbol.Small => "Small",
        SlotSymbol.Medium => "Medium",
        SlotSymbol.Large => "Large",
        SlotSymbol.Jackpot => "Jackpot",
        _ => "?"
    };


    /// <summary>
    /// Handles a single spin of the Slots game.
    /// Validates the request, spins three symbols, calculates the payout,
    /// updates the player's balance, writes Bet + Transaction records,
    /// and returns the result to the JavaScript client.
    /// </summary>


    // POST: /api/slots/spin
    [HttpPost("spin")]
    public async Task<IActionResult> Spin([FromForm] SpinRequestDto dto)
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

        // 2) Load game and ensure it's the Slots game and enabled
        var game = await _gameRepo.ReadAsync(dto.GameId);
        if (game == null)
        {
            return NotFound(new { message = "Game not found." });
        }

        if (!game.IsEnabled)
        {
            return BadRequest(new { message = "Game is disabled." });
        }

        // Verifies the game is Slots 
        if (!string.Equals(game.Name, "Slots", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Selected game is not a Slots game." });
        }

        var betAmount = dto.BetAmount;

        // 3) Validate bet against game limits and user balance
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

        // 4) Spin RNG for 3 symbols
        var (r1, r2, r3) = _rng.SpinThree();

        // 5) Calculate multiplier based on payout rules
        int multiplier = _payout.CalculateMultiplier(r1, r2, r3);
        var payoutAmount = betAmount * multiplier;

        // 6) Update user balance:
        // - subtract bet
        // - add payout if any
        user.DemoBalance -= betAmount;
        if (payoutAmount > 0)
        {
            user.DemoBalance += payoutAmount;
        }

        await _userRepo.UpdateAsync(user.Id, user);

        // 7) Create Bet record
        var bet = new Bet
        {
            UserId = user.Id,
            GameId = game.Id,
            BetAmount = betAmount,
            PayoutAmount = payoutAmount,
            Result = payoutAmount > 0 ? "Win" : "Lose",
            PlacedAt = DateTime.UtcNow
        };

        var createdBet = await _betRepo.CreateAsync(bet);

        // 8) Create Transactions:
        // - one for the bet
        // - one for payout (if win)
        var betTx = new Transaction
        {
            UserId = user.Id,
            Amount = betAmount,
            Type = TransactionType.Bet,
            Description = "Slots bet",
            BetId = createdBet.Id
        };

        await _transactionRepo.CreateAsync(betTx);

        if (payoutAmount > 0)
        {
            var payoutTx = new Transaction
            {
                UserId = user.Id,
                Amount = payoutAmount,
                Type = TransactionType.Payout,
                Description = "Slots payout",
                BetId = createdBet.Id
            };

            await _transactionRepo.CreateAsync(payoutTx);
        }

        // 9) Build display result for frontend
        string display = $"{SymbolToString(r1)} | {SymbolToString(r2)} | {SymbolToString(r3)}";

        var response = new SpinResultDto
        {
            Bet = ToBetReadDto(createdBet),
            NewBalance = user.DemoBalance,
            DisplayResult = display
        };

        return Ok(response);
    }
}

