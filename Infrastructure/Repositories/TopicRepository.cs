using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TopicRepository : ITopicRepository
{
    private readonly AppDbContext _context;

    public TopicRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Topic>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _context.Topics.ToListAsync(cancellationToken);
    }

    public Task<List<Topic>> GetByConferenceIdAsync(int conferenceId, CancellationToken cancellationToken = default)
    {
        return _context.Topics
            .Where(t => t.ConferenceId == conferenceId)
            .ToListAsync(cancellationToken);
    }

    public Task<Topic?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Topics.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public Task<List<Topic>> GetByIdsAsync(IEnumerable<int> topicIds, CancellationToken cancellationToken = default)
    {
        return _context.Topics
            .Where(t => topicIds.Contains(t.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Topics.AnyAsync(t => t.Id == id, cancellationToken);
    }

    public void Add(Topic topic)
    {
        _context.Topics.Add(topic);
    }

    public void Remove(Topic topic)
    {
        _context.Topics.Remove(topic);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

