using GamblingApp.Models.Entities;

namespace GamblingApp.Services.Repositories;

public interface IBetRepository
{
    Task<Bet?> ReadAsync(int id);
    Task<ICollection<Bet>> ReadByUserAsync(int userId);
    Task<ICollection<Bet>> ReadByUserAndGameAsync(int userId, int gameId);

    Task<Bet> CreateAsync(Bet bet);
}