using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Logging;
using Application.Mappings;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly IUserTopicRepository _userTopicRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ITopicRepository topicRepository,
        IUserTopicRepository userTopicRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _topicRepository = topicRepository;
        _userTopicRepository = userTopicRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return [.. users.Select(u => u.ToResponse())];
    }

    public async Task<UserResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(id, cancellationToken);
        return user.ToResponse();
    }

    public async Task<UserResponse> RegisterUserAsync(UserCreateDto dto, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(dto.Email);
        await EnsureEmailIsFreeAsync(email, excludeUserId: null, cancellationToken);

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = email
        };

        _userRepository.Add(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.UserRegistered(user.Id);
        return user.ToResponse();
    }

    public async Task UpdateAsync(int id, UserCreateDto dto, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(id, cancellationToken);

        var email = NormalizeEmail(dto.Email);
        await EnsureEmailIsFreeAsync(email, excludeUserId: id, cancellationToken);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = email;

        await _userRepository.SaveChangesAsync(cancellationToken);
        _logger.UserUpdated(id);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(id, cancellationToken);

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync(cancellationToken);
        _logger.UserDeleted(id);
    }

    public async Task RegisterToTopicAsync(UserTopicDto dto, CancellationToken cancellationToken = default)
    {
        var userExists = await _userRepository.ExistsAsync(dto.UserId, cancellationToken);
        var topicExists = await _topicRepository.ExistsAsync(dto.TopicId, cancellationToken);
        if (!userExists || !topicExists)
        {
            _logger.RegistrationTargetNotFound(dto.UserId, dto.TopicId, userExists, topicExists);
            throw new NotFoundException("User or Topic not found.");
        }

        var alreadyRegistered = await _userTopicRepository.ExistsAsync(dto.UserId, dto.TopicId, cancellationToken);
        if (alreadyRegistered)
        {
            _logger.UserAlreadyRegisteredForTopic(dto.UserId, dto.TopicId);
            throw new ConflictException("User is already registered for this topic.");
        }

        _userTopicRepository.Add(new UserTopic { UserId = dto.UserId, TopicId = dto.TopicId });
        await _userTopicRepository.SaveChangesAsync(cancellationToken);
        _logger.UserRegisteredForTopic(dto.UserId, dto.TopicId);
    }

    public async Task UnregisterFromTopicAsync(UserTopicDto dto, CancellationToken cancellationToken = default)
    {
        var userTopic = await _userTopicRepository.GetByUserAndTopicAsync(dto.UserId, dto.TopicId, cancellationToken);
        if (userTopic is null)
        {
            _logger.RegistrationNotFound(dto.UserId, dto.TopicId);
            throw new NotFoundException("Registration not found.");
        }

        _userTopicRepository.Remove(userTopic);
        await _userTopicRepository.SaveChangesAsync(cancellationToken);
        _logger.UserUnregisteredFromTopic(dto.UserId, dto.TopicId);
    }

    public async Task<IReadOnlyList<TopicResponse>> GetUserTopicsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userExists = await _userRepository.ExistsAsync(userId, cancellationToken);
        if (!userExists)
            throw UserNotFound(userId);

        var topics = await _userTopicRepository.GetTopicsByUserIdAsync(userId, cancellationToken);
        return [.. topics.Select(t => t.ToResponse())];
    }

    public async Task<IReadOnlyList<UserResponse>> GetUsersByTopicAsync(int topicId, CancellationToken cancellationToken = default)
    {
        var topicExists = await _topicRepository.ExistsAsync(topicId, cancellationToken);
        if (!topicExists)
        {
            _logger.TopicNotFound(topicId);
            throw new NotFoundException($"Topic with ID {topicId} not found.");
        }

        var users = await _userTopicRepository.GetUsersByTopicIdAsync(topicId, cancellationToken);
        return [.. users.Select(u => u.ToResponse())];
    }

    private async Task<User> GetUserAsync(int id, CancellationToken cancellationToken) =>
        await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw UserNotFound(id);

    private NotFoundException UserNotFound(int id)
    {
        _logger.UserNotFound(id);
        return new NotFoundException($"User with ID {id} not found.");
    }

    private async Task EnsureEmailIsFreeAsync(string email, int? excludeUserId, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(email, excludeUserId, cancellationToken))
        {
            _logger.UserEmailTaken(excludeUserId);
            throw new ConflictException("A user with this e-mail address already exists.");
        }
    }

    /// <summary>E-mail addresses are compared case-insensitively, so they are stored trimmed and in lowercase.</summary>
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
