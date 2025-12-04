using GamblingApp.Models.Entities;

namespace GamblingApp.Services.Repositories;

public interface IUserRepository
{
    Task<User?> ReadAsync(int id);
    Task<User?> ReadByEmailAsync(string email);
    Task<ICollection<User>> ReadAllAsync();

    Task<User> CreateAsync(User user);
    Task UpdateAsync(int id, User updatedUser);
    Task DeleteAsync(int id);

    Task<bool> EmailExistsAsync(string email);
}