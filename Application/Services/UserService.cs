using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly IUserTopicRepository _userTopicRepository;

    public UserService(
        IUserRepository userRepository,
        ITopicRepository topicRepository,
        IUserTopicRepository userTopicRepository)
    {
        _userRepository = userRepository;
        _topicRepository = topicRepository;
        _userTopicRepository = userTopicRepository;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User> RegisterUserAsync(UserCreateDto dto)
    {
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email
        };

        _userRepository.Add(user);
        await _userRepository.SaveChangesAsync();
        return user;
    }

    public async Task RegisterToTopicAsync(UserTopicDto dto)
    {
        var userExists = await _userRepository.ExistsAsync(dto.UserId);
        var topicExists = await _topicRepository.ExistsAsync(dto.TopicId);
        if (!userExists || !topicExists)
            throw new KeyNotFoundException("User or Topic not found.");

        var alreadyRegistered = await _userTopicRepository.ExistsAsync(dto.UserId, dto.TopicId);
        if (alreadyRegistered)
            throw new InvalidOperationException("User is already registered for this topic.");

        _userTopicRepository.Add(new UserTopic { UserId = dto.UserId, TopicId = dto.TopicId });
        await _userTopicRepository.SaveChangesAsync();
    }

    public async Task UnregisterFromTopicAsync(UserTopicDto dto)
    {
        var userTopic = await _userTopicRepository.GetByUserAndTopicAsync(dto.UserId, dto.TopicId)
            ?? throw new KeyNotFoundException("Registration not found.");

        _userTopicRepository.Remove(userTopic);
        await _userTopicRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<Topic>> GetUserTopicsAsync(int userId)
    {
        var userExists = await _userRepository.ExistsAsync(userId);
        if (!userExists)
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        return await _userTopicRepository.GetTopicsByUserIdAsync(userId);
    }

    public async Task<IEnumerable<User>> GetUsersByTopicAsync(int topicId)
    {
        var topicExists = await _topicRepository.ExistsAsync(topicId);
        if (!topicExists)
            throw new KeyNotFoundException($"Topic with ID {topicId} not found.");

        return await _userTopicRepository.GetUsersByTopicIdAsync(topicId);
    }
}