using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserTopicRepository : IUserTopicRepository
{
    private readonly AppDbContext _context;

    public UserTopicRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(int userId, int topicId, CancellationToken cancellationToken = default)
    {
        return _context.UserTopics.AnyAsync(ut => ut.UserId == userId && ut.TopicId == topicId, cancellationToken);
    }

    public Task<UserTopic?> GetByUserAndTopicAsync(int userId, int topicId, CancellationToken cancellationToken = default)
    {
        return _context.UserTopics
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TopicId == topicId, cancellationToken);
    }

    public Task<List<Topic>> GetTopicsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _context.UserTopics
            .Where(ut => ut.UserId == userId)
            .Select(ut => ut.Topic)
            .ToListAsync(cancellationToken);
    }

    public Task<List<User>> GetUsersByTopicIdAsync(int topicId, CancellationToken cancellationToken = default)
    {
        return _context.UserTopics
            .Where(ut => ut.TopicId == topicId)
            .Select(ut => ut.User)
            .ToListAsync(cancellationToken);
    }

    public void Add(UserTopic userTopic)
    {
        _context.UserTopics.Add(userTopic);
    }

    public void Remove(UserTopic userTopic)
    {
        _context.UserTopics.Remove(userTopic);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

