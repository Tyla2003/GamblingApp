using GamblingApp.Models.DTOs;
using GamblingApp.Models.Entities;
using GamblingApp.Models.Enums;
using GamblingApp.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers;

/// <summary>
/// Handles all wallet-related actions for users, including deposits
/// and retrieving transaction history. 
/// 
/// This controller simulates a real-world wallet system, allowing users
/// to add demo credits using their saved payment methods, and lets
/// an Admin add credit funds for testing.
/// 
/// Every wallet action automatically creates a Transaction record,
/// which supports the transaction log feature in the project.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly ITransactionRepository _transactionRepo;

    public WalletController(
        IUserRepository userRepo,
        ITransactionRepository transactionRepo)
    {
        _userRepo = userRepo;
        _transactionRepo = transactionRepo;
    }

    /// <summary>
    /// Converts a Transaction entity into a DTO sent back to the frontend.
    /// Helps keep API responses clean and avoids exposing unnecessary fields.
    /// </summary>

    private static TransactionReadDto ToReadDto(Transaction t) => new()
    {
        Id = t.Id,
        Amount = t.Amount,
        Type = t.Type,
        Description = t.Description,
        CreatedAt = t.CreatedAt,
        BetId = t.BetId
    };

    /// <summary>
    /// Allows a regular user to add demo funds to their account.
    /// This uses the "deposit" transaction type and is triggered from the
    /// Add Funds modal on the Account page.
    /// the flow simulates how a real payment system would work.
    /// </summary>
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromForm] WalletDepositDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userRepo.ReadAsync(dto.UserId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        // Update balance
        user.DemoBalance += dto.Amount;
        await _userRepo.UpdateAsync(user.Id, user);

        // Log transaction
        var tx = new Transaction
        {
            UserId = user.Id,
            Amount = dto.Amount,
            Type = TransactionType.Deposit,
            Description = string.IsNullOrWhiteSpace(dto.Description)
                ? "Wallet deposit"
                : dto.Description
        };

        var createdTx = await _transactionRepo.CreateAsync(tx);

        return Ok(new
        {
            Message = "Deposit successful.",
            NewBalance = user.DemoBalance,
            Transaction = ToReadDto(createdTx)
        });
    }

    /// <summary>
    /// Admin only credit function used for testing, seeding balances,
    /// or adjusting a user's demo funds. This creates an "AdminCredit"
    /// transaction so the action is logged.
    /// </summary>
    [HttpPost("admin-credit")]
    public async Task<IActionResult> AdminCredit([FromForm] AdminCreditDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userRepo.ReadAsync(dto.UserId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.DemoBalance += dto.Amount;
        await _userRepo.UpdateAsync(user.Id, user);

        var tx = new Transaction
        {
            UserId = user.Id,
            Amount = dto.Amount,
            Type = TransactionType.AdminCredit,
            Description = string.IsNullOrWhiteSpace(dto.Reason)
                ? "Admin credit"
                : dto.Reason
        };

        var createdTx = await _transactionRepo.CreateAsync(tx);

        return Ok(new
        {
            Message = "Admin credit applied.",
            NewBalance = user.DemoBalance,
            Transaction = ToReadDto(createdTx)
        });
    }

    /// <summary>
    /// Returns the entire transaction history for a specific user.
    /// Used on the Admin panel when viewing a user's account.
    /// </summary>
    [HttpGet("user/{userId:int}/transactions")]
    public async Task<IActionResult> GetAllForUser(int userId)
    {
        var user = await _userRepo.ReadAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var txs = await _transactionRepo.ReadByUserAsync(userId);
        var dtos = txs.Select(ToReadDto).ToList();

        return Ok(dtos);
    }
}