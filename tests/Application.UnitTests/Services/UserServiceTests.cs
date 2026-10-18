using Application.DTOs;
using Application.Services;
using Application.UnitTests.Fakes;

namespace Application.UnitTests.Services;

public sealed class UserServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeTopicRepository _topics = new();
    private readonly FakeUserTopicRepository _registrations;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _registrations = new FakeUserTopicRepository(_users, _topics);
        _sut = new UserService(_users, _topics, _registrations);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        _users.Seed("Ana");
        _users.Seed("Marko");

        var result = await _sut.GetAllAsync();

        Assert.Equal(["Ana", "Marko"], result.Select(u => u.FirstName));
    }

    [Fact]
    public async Task RegisterUserAsync_CopiesFieldsAndSaves()
    {
        var dto = new UserCreateDto { FirstName = "Ana", LastName = "Anić", Email = "ana@example.com" };

        var user = await _sut.RegisterUserAsync(dto);

        Assert.Single(_users.Items);
        Assert.Equal(dto.FirstName, user.FirstName);
        Assert.Equal(dto.LastName, user.LastName);
        Assert.Equal(dto.Email, user.Email);
        Assert.Equal(1, _users.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterToTopicAsync_ValidUserAndTopic_AddsRegistration()
    {
        var user = _users.Seed();
        var topic = _topics.Seed(conferenceId: 1);

        await _sut.RegisterToTopicAsync(new UserTopicDto { UserId = user.Id, TopicId = topic.Id });

        var registration = Assert.Single(_registrations.Items);
        Assert.Equal(user.Id, registration.UserId);
        Assert.Equal(topic.Id, registration.TopicId);
        Assert.Equal(1, _registrations.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterToTopicAsync_MissingUser_Throws()
    {
        var topic = _topics.Seed(conferenceId: 1);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.RegisterToTopicAsync(new UserTopicDto { UserId = 42, TopicId = topic.Id }));

        Assert.Empty(_registrations.Items);
    }

    [Fact]
    public async Task RegisterToTopicAsync_MissingTopic_Throws()
    {
        var user = _users.Seed();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.RegisterToTopicAsync(new UserTopicDto { UserId = user.Id, TopicId = 42 }));

        Assert.Empty(_registrations.Items);
    }

    [Fact]
    public async Task RegisterToTopicAsync_AlreadyRegistered_ThrowsAndDoesNotDuplicate()
    {
        var user = _users.Seed();
        var topic = _topics.Seed(conferenceId: 1);
        _registrations.Seed(user.Id, topic.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.RegisterToTopicAsync(new UserTopicDto { UserId = user.Id, TopicId = topic.Id }));

        Assert.Single(_registrations.Items);
        Assert.Equal(0, _registrations.SaveChangesCalls);
    }

    [Fact]
    public async Task UnregisterFromTopicAsync_ExistingRegistration_RemovesIt()
    {
        var user = _users.Seed();
        var topic = _topics.Seed(conferenceId: 1);
        _registrations.Seed(user.Id, topic.Id);

        await _sut.UnregisterFromTopicAsync(new UserTopicDto { UserId = user.Id, TopicId = topic.Id });

        Assert.Empty(_registrations.Items);
        Assert.Equal(1, _registrations.SaveChangesCalls);
    }

    [Fact]
    public async Task UnregisterFromTopicAsync_MissingRegistration_Throws()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UnregisterFromTopicAsync(new UserTopicDto { UserId = 1, TopicId = 1 }));

        Assert.Equal(0, _registrations.SaveChangesCalls);
    }

    [Fact]
    public async Task GetUserTopicsAsync_ReturnsRegisteredTopicsOnly()
    {
        var user = _users.Seed();
        var registered = _topics.Seed(conferenceId: 1, "Registered");
        _topics.Seed(conferenceId: 1, "Other");
        _registrations.Seed(user.Id, registered.Id);

        var result = await _sut.GetUserTopicsAsync(user.Id);

        Assert.Equal([registered.Id], result.Select(t => t.Id));
    }

    [Fact]
    public async Task GetUserTopicsAsync_ExistingUserWithoutRegistrations_ReturnsEmpty()
    {
        var user = _users.Seed();

        var result = await _sut.GetUserTopicsAsync(user.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserTopicsAsync_MissingUser_Throws()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetUserTopicsAsync(42));
    }

    [Fact]
    public async Task GetUsersByTopicAsync_ReturnsRegisteredUsersOnly()
    {
        var topic = _topics.Seed(conferenceId: 1);
        var registered = _users.Seed("Ana");
        _users.Seed("Marko");
        _registrations.Seed(registered.Id, topic.Id);

        var result = await _sut.GetUsersByTopicAsync(topic.Id);

        Assert.Equal([registered.Id], result.Select(u => u.Id));
    }

    [Fact]
    public async Task GetUsersByTopicAsync_MissingTopic_Throws()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetUsersByTopicAsync(42));
    }
}

