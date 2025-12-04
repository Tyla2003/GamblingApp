using System.ComponentModel.DataAnnotations;

/// <summary>
/// Incoming login credentials for authentication.
/// This DTO feeds the Login endpoint to validate email + password.
/// </summary>

namespace GamblingApp.Models.DTOs;

public class UserLoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}