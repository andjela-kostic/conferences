using Application.Interfaces;
using Domain.Entities;

namespace Application.UnitTests.Fakes;

public sealed class FakeUserTopicRepository(FakeUserRepository users, FakeTopicRepository topics) : IUserTopicRepository
{
    private int _nextId = 1;

    public List<UserTopic> Items { get; } = [];

    public int SaveChangesCalls { get; private set; }

    public UserTopic Seed(int userId, int topicId)
    {
        var userTopic = new UserTopic { Id = _nextId++, UserId = userId, TopicId = topicId };
        Items.Add(userTopic);
        return userTopic;
    }

    public Task<bool> ExistsAsync(int userId, int topicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(ut => ut.UserId == userId && ut.TopicId == topicId));

    public Task<UserTopic?> GetByUserAndTopicAsync(int userId, int topicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(ut => ut.UserId == userId && ut.TopicId == topicId));

    public Task<List<Topic>> GetTopicsByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items
            .Where(ut => ut.UserId == userId)
            .Join(topics.Items, ut => ut.TopicId, t => t.Id, (_, t) => t)
            .ToList());

    public Task<List<User>> GetUsersByTopicIdAsync(int topicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items
            .Where(ut => ut.TopicId == topicId)
            .Join(users.Items, ut => ut.UserId, u => u.Id, (_, u) => u)
            .ToList());

    public void Add(UserTopic userTopic)
    {
        userTopic.Id = _nextId++;
        Items.Add(userTopic);
    }

    public void Remove(UserTopic userTopic) => Items.Remove(userTopic);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(0);
    }
}

