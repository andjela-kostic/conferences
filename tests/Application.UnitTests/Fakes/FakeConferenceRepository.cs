using Application.Interfaces;
using Domain.Entities;

namespace Application.UnitTests.Fakes;

public sealed class FakeConferenceRepository : IConferenceRepository
{
    private int _nextId = 1;

    public List<Conference> Items { get; } = [];

    public int SaveChangesCalls { get; private set; }

    public Conference Seed(string name = "Conference")
    {
        var conference = new Conference { Id = _nextId++, Name = name, Description = $"{name} description" };
        Items.Add(conference);
        return conference;
    }

    public Task<List<Conference>> GetAllWithTopicsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.ToList());

    public Task<Conference?> GetByIdWithTopicsAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(c => c.Id == id));

    public Task<Conference?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(c => c.Id == id));

    public void Add(Conference conference)
    {
        conference.Id = _nextId++;
        Items.Add(conference);
    }

    public void Remove(Conference conference) => Items.Remove(conference);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(0);
    }
}

