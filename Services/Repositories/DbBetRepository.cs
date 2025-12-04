using GamblingApp.Data;
using GamblingApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamblingApp.Services.Repositories;

/// <summary>
/// Repository for accessing and creating Bet records.
/// Handles loading related Game/User entities and retrieving bet history.
/// </summary>

public class DbBetRepository : IBetRepository
{
    private readonly ApplicationDbContext _db;

    public DbBetRepository(ApplicationDbContext db)
    {
        _db = db;
    }


    /// <summary>
    /// Returns a single bet by id, including related User and Game.
    /// </summary>

    public async Task<Bet?> ReadAsync(int id)
        => await _db.Bets
            .Include(b => b.Game)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);


    /// <summary>
    /// Returns all bets placed by a user, ordered newest → oldest.
    /// </summary>

    public async Task<ICollection<Bet>> ReadByUserAsync(int userId)
        => await _db.Bets
            .Include(b => b.Game)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.PlacedAt)
            .ToListAsync();


    /// <summary>
    /// Returns bets for a specific user filtered to a specific game.
    /// </summary>

    public async Task<ICollection<Bet>> ReadByUserAndGameAsync(int userId, int gameId)
        => await _db.Bets
            .Where(b => b.UserId == userId && b.GameId == gameId)
            .OrderByDescending(b => b.PlacedAt)
            .ToListAsync();

    public async Task<Bet> CreateAsync(Bet bet)
    {
        await _db.Bets.AddAsync(bet);
        await _db.SaveChangesAsync();
        return bet;
    }
}