using Domain.Entities;

namespace Application.Interfaces;

public interface IConferenceRepository
{
    Task<List<Conference>> GetAllWithTopicsAsync(CancellationToken cancellationToken = default);
    Task<Conference?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(Conference conference);
    void Remove(Conference conference);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

