using GamblingApp.Models.DTOs;
using GamblingApp.Models.Entities;
using GamblingApp.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers;

/// <summary>
/// Handles all REST API operations related to a user's payment methods.
/// 
///  This controller allows users to:
///  View their saved cards
///  Add a new payment method
///  Update an existing card
///  Delete a payment method
///
/// Although the app uses "demo credits," this controller simulates
/// real world wallet functionality and is used for the Add Funds workflow.
/// 
/// All logic is server-side validated, including card expiration checks.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodController : ControllerBase
{
    private readonly IPaymentMethodRepository _paymentRepo;
    private readonly IUserRepository _userRepo;

    public PaymentMethodController(
        IPaymentMethodRepository paymentRepo,
        IUserRepository userRepo)
    {
        _paymentRepo = paymentRepo;
        _userRepo = userRepo;
    }

    /// <summary>
    /// Helper method that masks a card number so only the last 4 digits show.
    /// This keeps the API responses safe to display in the UI.
    /// </summary>
    private static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return string.Empty;

        var trimmed = cardNumber.Replace(" ", "").Replace("-", "");

        if (trimmed.Length <= 4)
            return trimmed;

        var last4 = trimmed[^4..];
        return $"**** **** **** {last4}";
    }

    /// <summary>
    /// Converts a PaymentMethod entity into a DTO sent to the frontend.
    /// </summary>
    private static PaymentMethodReadDto ToReadDto(PaymentMethod pm) => new()
    {
        Id = pm.Id,
        CardholderName = pm.CardholderName,
        MaskedCardNumber = MaskCardNumber(pm.CardNumber),
        ExpMonth = pm.ExpMonth,
        ExpYear = pm.ExpYear,
        IsActive = pm.IsActive,
        Nickname = pm.Nickname
    };

    /// <summary>
    /// Returns all payment methods saved by a specific user.
    /// Used for the "View / Add Payment Methods" modal.
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetForUser(int userId)
    {
        var user = await _userRepo.ReadAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var methods = await _paymentRepo.ReadByUserAsync(userId);
        var dtos = methods.Select(ToReadDto).ToList();
        return Ok(dtos);
    }


    /// <summary>
    /// Returns a single payment method by its ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var method = await _paymentRepo.ReadAsync(id);
        if (method == null)
        {
            return NotFound();
        }

        return Ok(ToReadDto(method));
    }

    /// <summary>
    /// Creates a new payment method for a user. 
    /// Validates that the card is not expired and 
    /// stores only required card metadata.
    /// </summary>
    [HttpPost("user/{userId:int}")]
    public async Task<IActionResult> CreateForUser(
        int userId,
        [FromForm] PaymentMethodCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userRepo.ReadAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        // basic expiry validation
        var now = DateTime.UtcNow;
        if (dto.ExpYear < now.Year ||
            (dto.ExpYear == now.Year && dto.ExpMonth < now.Month))
        {
            return BadRequest(new { message = "Card is already expired." });
        }

        var method = new PaymentMethod
        {
            UserId = userId,
            CardholderName = dto.CardholderName,
            CardNumber = dto.CardNumber,
            ExpMonth = dto.ExpMonth,
            ExpYear = dto.ExpYear,
            Nickname = dto.Nickname,
            Cvv = dto.Cvv,
            IsActive = true
        };

        var created = await _paymentRepo.CreateAsync(method);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ToReadDto(created));
    }

    /// <summary>
    /// Updates an existing payment method.
    /// Allows users to rename, replace, or deactivate a card.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] PaymentMethodUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _paymentRepo.ReadAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // expiry check
        var now = DateTime.UtcNow;
        if (dto.ExpYear < now.Year ||
            (dto.ExpYear == now.Year && dto.ExpMonth < now.Month))
        {
            return BadRequest(new { message = "Card is already expired." });
        }

        existing.CardholderName = dto.CardholderName;
        existing.CardNumber = dto.CardNumber;
        existing.ExpMonth = dto.ExpMonth;
        existing.ExpYear = dto.ExpYear;
        existing.Nickname = dto.Nickname;
        existing.Cvv = dto.Cvv;
        existing.IsActive = dto.IsActive;

        await _paymentRepo.UpdateAsync(id, existing);

        return NoContent();
    }

    /// <summary>
    /// Deletes a saved payment method.
    /// Used when users clean up old or expired cards.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _paymentRepo.ReadAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        await _paymentRepo.DeleteAsync(id);
        return NoContent();
    }
}