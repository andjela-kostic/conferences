using Domain.Entities;

namespace Application.Interfaces;

public interface ITopicRepository
{
    Task<List<Topic>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Topic>> GetByConferenceIdAsync(int conferenceId, CancellationToken cancellationToken = default);
    Task<Topic?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Topic>> GetByIdsAsync(IEnumerable<int> topicIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    void Add(Topic topic);
    void Remove(Topic topic);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

