using Application.DTOs;

namespace Application.Interfaces;

public interface ITopicService
{
    Task<IReadOnlyList<TopicResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TopicResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopicResponse>> GetByConferenceAsync(int conferenceId, CancellationToken cancellationToken = default);
    Task<TopicResponse> CreateAsync(TopicCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, TopicCreateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
