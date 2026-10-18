using Application.Interfaces;
using Domain.Entities;

namespace Application.UnitTests.Fakes;

/// <summary>In-memory <see cref="IUserRepository"/> for service tests.</summary>
public sealed class FakeUserRepository : IUserRepository
{
    private int _nextId = 1;

    public List<User> Items { get; } = [];

    public int SaveChangesCalls { get; private set; }

    public User Seed(string firstName = "Ana")
    {
        var user = new User
        {
            Id = _nextId++,
            FirstName = firstName,
            LastName = "Test",
            Email = $"{firstName.ToUpperInvariant()}@example.com",
        };
        Items.Add(user);
        return user;
    }

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.ToList());

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(u => u.Id == id));

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(u => u.Id == id));

    public void Add(User user)
    {
        user.Id = _nextId++;
        Items.Add(user);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(0);
    }
}

