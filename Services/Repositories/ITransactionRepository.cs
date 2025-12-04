using GamblingApp.Models.Entities;

namespace GamblingApp.Services.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> ReadAsync(int id);
    Task<ICollection<Transaction>> ReadByUserAsync(int userId);
    Task<ICollection<Transaction>> ReadRecentByUserAsync(int userId, int count = 20);

    Task<Transaction> CreateAsync(Transaction transaction);
}