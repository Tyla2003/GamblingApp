using GamblingApp.Data;
using GamblingApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamblingApp.Services.Repositories;

/// <summary>
/// Repository managing the many-to-many relationship between Users and Games.
/// Handles adding, removing, and checking favorites.
/// </summary>

public class DbUserFavoriteRepository : IUserFavoriteRepository
{
    private readonly ApplicationDbContext _db;

    public DbUserFavoriteRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddFavoriteAsync(int userId, int gameId)
    {
        var exists = await _db.UserGameFavorites
            .AnyAsync(f => f.UserId == userId && f.GameId == gameId);

        if (exists) return;

        await _db.UserGameFavorites.AddAsync(new UserGameFavorite
        {
            UserId = userId,
            GameId = gameId
        });

        await _db.SaveChangesAsync();
    }

    public async Task RemoveFavoriteAsync(int userId, int gameId)
    {
        var fav = await _db.UserGameFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.GameId == gameId);

        if (fav == null) return;

        _db.UserGameFavorites.Remove(fav);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> IsFavoriteAsync(int userId, int gameId)
        => await _db.UserGameFavorites
            .AnyAsync(f => f.UserId == userId && f.GameId == gameId);

    public async Task<ICollection<Game>> ReadFavoritesForUserAsync(int userId)
        => await _db.UserGameFavorites
            .Where(f => f.UserId == userId)
            .Select(f => f.Game!)
            .ToListAsync();
}