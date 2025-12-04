using GamblingApp.Models.DTOs;
using GamblingApp.Models.Entities;
using GamblingApp.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers;

/// <summary>
/// Exposes basic CRUD endpoints for the Game entity.
/// Right now the app mainly uses the read endpoints, but the
/// create / update / delete actions are in place for future
/// admin tools or game management features.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameRepository _gameRepo;

    /// <summary>
    /// Injects the game repository so the controller can load
    /// and manage Game records from the database.
    /// </summary>
    /// 
    public GameController(IGameRepository gameRepo)
    {
        _gameRepo = gameRepo;
    }
    /// <summary>
    /// Maps a Game entity into a simple DTO used by the API and frontend.
    /// </summary>
    private static GameReadDto ToReadDto(Game g) => new()
    {
        Id = g.Id,
        Name = g.Name,
        Description = g.Description,
        MinBet = g.MinBet,
        MaxBet = g.MaxBet,
        IsEnabled = g.IsEnabled
    };

    /// <summary>
    /// Returns all games in the system. This can be used by the lobby
    /// or by future admin screens to list every available game.
    /// </summary>

    // GET: /api/game
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var games = await _gameRepo.ReadAllAsync();
        var dtos = games.Select(ToReadDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Returns a single game by its Id.
    /// </summary>

    // GET: /api/game/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var game = await _gameRepo.ReadAsync(id);
        if (game == null)
        {
            return NotFound();
        }

        return Ok(ToReadDto(game));
    }

    /// <summary>
    /// Creates a new game record, however currently not in use. 
    /// </summary>

    // POST: /api/game
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] GameCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var game = new Game
        {
            Name = dto.Name,
            Description = dto.Description,
            MinBet = dto.MinBet,
            MaxBet = dto.MaxBet,
            IsEnabled = dto.IsEnabled
        };

        var created = await _gameRepo.CreateAsync(game);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ToReadDto(created));
    }

    /// <summary>
    /// Updates an existing game with new values.
    /// However not in use just was added during creation.
    /// </summary>

    // PUT: /api/game/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] GameUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _gameRepo.ReadAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.MinBet = dto.MinBet;
        existing.MaxBet = dto.MaxBet;
        existing.IsEnabled = dto.IsEnabled;

        await _gameRepo.UpdateAsync(id, existing);

        return NoContent();
    }


    /// <summary>
    /// Deletes an existing game. 
    /// However not in use just was added during creation.
    /// No plans to implement, but the possiblity exists
    /// </summary>

    // DELETE: /api/game/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _gameRepo.ReadAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        await _gameRepo.DeleteAsync(id);
        return NoContent();
    }
}