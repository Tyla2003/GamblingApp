using GamblingApp.Models.Entities;

namespace GamblingApp.Services.Repositories;

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> ReadAsync(int id);
    Task<ICollection<PaymentMethod>> ReadByUserAsync(int userId);

    Task<PaymentMethod> CreateAsync(PaymentMethod method);
    Task UpdateAsync(int id, PaymentMethod updatedMethod);
    Task DeleteAsync(int id);
}