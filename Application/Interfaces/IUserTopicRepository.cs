using Domain.Entities;

namespace Application.Interfaces;

public interface IUserTopicRepository
{
    Task<bool> ExistsAsync(int userId, int topicId, CancellationToken cancellationToken = default);
    Task<UserTopic?> GetByUserAndTopicAsync(int userId, int topicId, CancellationToken cancellationToken = default);
    Task<List<Topic>> GetTopicsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<User>> GetUsersByTopicIdAsync(int topicId, CancellationToken cancellationToken = default);
    void Add(UserTopic userTopic);
    void Remove(UserTopic userTopic);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

