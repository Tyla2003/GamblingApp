using System.ComponentModel.DataAnnotations;

/// <summary>
/// Input model for updating an existing game.  
/// Mirrors GameCreateDto but is used in PUT requests instead.
/// </summary>

namespace GamblingApp.Models.DTOs;

public class GameUpdateDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    [Range(1, 1_000_000)]
    public decimal MinBet { get; set; }

    [Range(1, 1_000_000)]
    public decimal MaxBet { get; set; }

    public bool IsEnabled { get; set; }
}