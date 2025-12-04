using GamblingApp.Data;
using GamblingApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamblingApp.Services.Repositories;

/// <summary>
/// Repository handling all CRUD operations for User accounts,
/// including balance updates, admin role changes, and credentials.
/// </summary>

public class DbUserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public DbUserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<User?> ReadAsync(int id)
        => await _db.Users.FindAsync(id);

    public async Task<User?> ReadByEmailAsync(string email)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<ICollection<User>> ReadAllAsync()
        => await _db.Users.ToListAsync();

    public async Task<User> CreateAsync(User user)
    {
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
        return user;
    }


    /// <summary>
    /// Updates profile info, password, balance, or admin role.
    /// </summary>
    public async Task UpdateAsync(int id, User updatedUser)
    {
        var existing = await ReadAsync(id);
        if (existing == null) return;

        existing.FirstName = updatedUser.FirstName;
        existing.LastName = updatedUser.LastName;
        existing.Email = updatedUser.Email;
        existing.Password = updatedUser.Password;
        existing.DemoBalance = updatedUser.DemoBalance;
        existing.IsAdmin = updatedUser.IsAdmin;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var user = await ReadAsync(id);
        if (user == null) return;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
        => await _db.Users.AnyAsync(u => u.Email == email);
}