using GamblingApp.Models.Entities;

namespace GamblingApp.Services.Repositories;

public interface IUserFavoriteRepository
{
    Task AddFavoriteAsync(int userId, int gameId);
    Task RemoveFavoriteAsync(int userId, int gameId);
    Task<bool> IsFavoriteAsync(int userId, int gameId);
    Task<ICollection<Game>> ReadFavoritesForUserAsync(int userId);
}