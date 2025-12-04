using GamblingApp.Data;
using GamblingApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamblingApp.Services.Repositories;

/// <summary>
/// Repository for reading and writing wallet and betting transactions.
/// Includes deposit logs, bet logs, and payout logs.
/// </summary>

public class DbTransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _db;

    public DbTransactionRepository(ApplicationDbContext db)
    {
        _db = db;
    }


    /// <summary>
    /// Loads a transaction and its related User and optional Bet.
    /// </summary>
    public async Task<Transaction?> ReadAsync(int id)
        => await _db.Transactions
            .Include(t => t.User)
            .Include(t => t.Bet)
            .FirstOrDefaultAsync(t => t.Id == id);


    /// <summary>
    /// Full transaction history for a user, newest first.
    /// </summary>
    public async Task<ICollection<Transaction>> ReadByUserAsync(int userId)
        => await _db.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<ICollection<Transaction>> ReadRecentByUserAsync(int userId, int count = 20)
        => await _db.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        await _db.Transactions.AddAsync(transaction);
        await _db.SaveChangesAsync();
        return transaction;
    }
}