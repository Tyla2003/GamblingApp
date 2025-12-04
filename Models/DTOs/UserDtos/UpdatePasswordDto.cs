namespace GamblingApp.Models.DTOs
{

/// <summary>
/// Used when a user changes their password.
/// They must confirm their current password for validation.
/// </summary>
    public class UpdatePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}