using Application.Interfaces;
using Domain.Entities;

namespace Application.UnitTests.Fakes;

public sealed class FakeTopicRepository : ITopicRepository
{
    private int _nextId = 1;

    public List<Topic> Items { get; } = [];

    public int SaveChangesCalls { get; private set; }

    public Topic Seed(int conferenceId, string name = "Topic")
    {
        var topic = new Topic { Id = _nextId++, Name = name, Description = $"{name} description", ConferenceId = conferenceId };
        Items.Add(topic);
        return topic;
    }

    public Task<List<Topic>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.ToList());

    public Task<List<Topic>> GetByConferenceIdAsync(int conferenceId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Where(t => t.ConferenceId == conferenceId).ToList());

    public Task<Topic?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(t => t.Id == id));

    public Task<List<Topic>> GetByIdsAsync(IEnumerable<int> topicIds, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Where(t => topicIds.Contains(t.Id)).ToList());

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(t => t.Id == id));

    public void Add(Topic topic)
    {
        topic.Id = _nextId++;
        Items.Add(topic);
    }

    public void Remove(Topic topic) => Items.Remove(topic);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(0);
    }
}

