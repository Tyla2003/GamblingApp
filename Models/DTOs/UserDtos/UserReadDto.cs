namespace GamblingApp.Models.DTOs;

/// <summary>
/// Outbound user data returned after login, registration,
/// or any GET request for user info.  
/// This is the safe, public-facing version of a User entity.
/// </summary>

public class UserReadDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal DemoBalance { get; set; }
    public bool IsAdmin { get; set; }

    public DateTime CreatedAt { get; set; }
}