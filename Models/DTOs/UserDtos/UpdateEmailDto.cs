namespace GamblingApp.Models.DTOs
{

/// <summary>
/// Used when a logged-in user wants to update their email.
/// Requires their current password since we are not using tokens.
/// </summary>
    public class UpdateEmailDto
    {
        public string NewEmail { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
    }
}