using GamblingApp.Models.DTOs;
using GamblingApp.Models.Entities;
using GamblingApp.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers;

/// <summary>
/// API controller for managing each user's favorite games.
/// This wires the many-to-many relationship between Users and Games
/// through the UserGameFavorite join table.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly IUserFavoriteRepository _favoriteRepo;
    private readonly IUserRepository _userRepo;
    private readonly IGameRepository _gameRepo;

    public FavoritesController(
        IUserFavoriteRepository favoriteRepo,
        IUserRepository userRepo,
        IGameRepository gameRepo)
    {
        _favoriteRepo = favoriteRepo;
        _userRepo = userRepo;
        _gameRepo = gameRepo;
    }

    private static GameReadDto ToGameReadDto(Game g) => new()
    {
        Id = g.Id,
        Name = g.Name,
        Description = g.Description,
        MinBet = g.MinBet,
        MaxBet = g.MaxBet,
        IsEnabled = g.IsEnabled
    };

    /// <summary>
    /// Returns all games that a given user has marked as favorite.
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetFavoritesForUser(int userId)
    {
        var user = await _userRepo.ReadAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var games = await _favoriteRepo.ReadFavoritesForUserAsync(userId);
        var dtos = games.Select(ToGameReadDto).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Adds a game to a user's favorites list if it is not already present.
    /// </summary>
    [HttpPost("user/{userId:int}/game/{gameId:int}")]
    public async Task<IActionResult> AddFavorite(int userId, int gameId)
    {
        var user = await _userRepo.ReadAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var game = await _gameRepo.ReadAsync(gameId);
        if (game == null)
        {
            return NotFound(new { message = "Game not found." });
        }

        await _favoriteRepo.AddFavoriteAsync(userId, gameId);

        return Ok(new { message = "Game added to favorites." });
    }


    /// <summary>
    /// Removes a game from a user's favorites list if it is currently favorited.
    /// </summary>
    [HttpDelete("user/{userId:int}/game/{gameId:int}")]
    public async Task<IActionResult> RemoveFavorite(int userId, int gameId)
    {
        var user = await _userRepo.ReadAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var game = await _gameRepo.ReadAsync(gameId);
        if (game == null)
        {
            return NotFound(new { message = "Game not found." });
        }

        await _favoriteRepo.RemoveFavoriteAsync(userId, gameId);

        return Ok(new { message = "Game removed from favorites." });
    }
}