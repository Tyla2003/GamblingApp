namespace GamblingApp.Models.DTOs;

/// <summary>
/// Read-only version of a payment method.
/// Returned to the frontend any time card details need to be listed,
/// with the number safely masked so no sensitive info is exposed.
/// </summary>

public class PaymentMethodReadDto
{
    public int Id { get; set; }
    public string CardholderName { get; set; } = string.Empty;
    public string MaskedCardNumber { get; set; } = string.Empty; // e.g. **** **** **** 1234
    public int ExpMonth { get; set; }
    public int ExpYear { get; set; }
    public string? Nickname { get; set; } 
    public bool IsActive { get; set; }
}