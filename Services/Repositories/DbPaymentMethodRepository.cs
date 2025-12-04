using GamblingApp.Data;
using GamblingApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamblingApp.Services.Repositories;

/// <summary>
/// Repository for storing and updating user payment methods.
/// Used for simulated deposits in the demo wallet.
/// </summary>

public class DbPaymentMethodRepository : IPaymentMethodRepository
{
    private readonly ApplicationDbContext _db;

    public DbPaymentMethodRepository(ApplicationDbContext db)
    {
        _db = db;
    }


    /// <summary>
    /// Loads a payment method with its associated user.
    /// </summary>
    public async Task<PaymentMethod?> ReadAsync(int id)
        => await _db.PaymentMethods
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm => pm.Id == id);



    /// <summary>
    /// Returns all active payment methods for a user.
    /// </summary>
    public async Task<ICollection<PaymentMethod>> ReadByUserAsync(int userId)
        => await _db.PaymentMethods
            .Where(pm => pm.UserId == userId && pm.IsActive)
            .ToListAsync();

    public async Task<PaymentMethod> CreateAsync(PaymentMethod method)
    {
        await _db.PaymentMethods.AddAsync(method);
        await _db.SaveChangesAsync();
        return method;
    }

    public async Task UpdateAsync(int id, PaymentMethod updatedMethod)
    {
        var existing = await ReadAsync(id);
        if (existing == null) return;

        existing.CardholderName = updatedMethod.CardholderName;
        existing.CardNumber = updatedMethod.CardNumber;
        existing.ExpMonth = updatedMethod.ExpMonth;
        existing.ExpYear = updatedMethod.ExpYear;
        existing.Cvv = updatedMethod.Cvv;
        existing.IsActive = updatedMethod.IsActive;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await ReadAsync(id);
        if (existing == null) return;

        _db.PaymentMethods.Remove(existing);
        await _db.SaveChangesAsync();
    }
}