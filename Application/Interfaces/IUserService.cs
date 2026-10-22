using Application.DTOs;

namespace Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserResponse> RegisterUserAsync(UserCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UserCreateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task RegisterToTopicAsync(UserTopicDto dto, CancellationToken cancellationToken = default);
    Task UnregisterFromTopicAsync(UserTopicDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopicResponse>> GetUserTopicsAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserResponse>> GetUsersByTopicAsync(int topicId, CancellationToken cancellationToken = default);
}
