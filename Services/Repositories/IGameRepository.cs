using GamblingApp.Models.Entities;

namespace GamblingApp.Services.Repositories;

public interface IGameRepository
{
    Task<ICollection<Game>> ReadAllAsync();
    Task<Game?> ReadAsync(int id);
    Task<Game?> ReadByNameAsync(string name);

    Task<Game> CreateAsync(Game game);
    Task UpdateAsync(int id, Game updatedGame);
    Task DeleteAsync(int id);
}