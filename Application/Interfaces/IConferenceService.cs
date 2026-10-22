using Application.DTOs;

namespace Application.Interfaces;

public interface IConferenceService
{
    Task<IReadOnlyList<ConferenceResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConferenceResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ConferenceResponse> CreateAsync(ConferenceCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, ConferenceCreateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
