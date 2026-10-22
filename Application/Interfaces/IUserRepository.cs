using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Whether another user already uses <paramref name="email"/> (expected to be normalized).</summary>
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default);
    void Add(User user);
    void Remove(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

