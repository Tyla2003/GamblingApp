using GamblingApp.Data;
using GamblingApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamblingApp.Services.Repositories;

/// <summary>
/// Repository providing CRUD operations for casino games.
/// Games are used to configure betting limits and enable/disable features.
/// </summary>

public class DbGameRepository : IGameRepository
{
    private readonly ApplicationDbContext _db;

    public DbGameRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ICollection<Game>> ReadAllAsync()
        => await _db.Games.ToListAsync();

    public async Task<Game?> ReadAsync(int id)
        => await _db.Games.FindAsync(id);


    /// <summary>
    /// Looks up a game by its exact name ( "Slots" or "Blackjack").
    /// </summary>
    public async Task<Game?> ReadByNameAsync(string name)
        => await _db.Games.FirstOrDefaultAsync(g => g.Name == name);

    public async Task<Game> CreateAsync(Game game)
    {
        await _db.Games.AddAsync(game);
        await _db.SaveChangesAsync();
        return game;
    }

    public async Task UpdateAsync(int id, Game updatedGame)
    {
        var existing = await ReadAsync(id);
        if (existing == null) return;

        existing.Name = updatedGame.Name;
        existing.Description = updatedGame.Description;
        existing.MinBet = updatedGame.MinBet;
        existing.MaxBet = updatedGame.MaxBet;
        existing.IsEnabled = updatedGame.IsEnabled;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var game = await ReadAsync(id);
        if (game == null) return;

        _db.Games.Remove(game);
        await _db.SaveChangesAsync();
    }
}