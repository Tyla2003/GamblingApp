using System.ComponentModel.DataAnnotations;

/// <summary>
/// Incoming data for user registration.  
/// Validates basic formatting and ensures the account starts with required fields.
/// </summary>

namespace GamblingApp.Models.DTOs;

public class UserRegisterDto
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}