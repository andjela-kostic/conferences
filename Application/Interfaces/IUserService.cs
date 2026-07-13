using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> RegisterUserAsync(UserCreateDto dto);
    Task RegisterToTopicAsync(UserTopicDto dto);
    Task UnregisterFromTopicAsync(UserTopicDto dto);
    Task<IEnumerable<Topic>> GetUserTopicsAsync(int userId);
    Task<IEnumerable<User>> GetUsersByTopicAsync(int topicId);
}