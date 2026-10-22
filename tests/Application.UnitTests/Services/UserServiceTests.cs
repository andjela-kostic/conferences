using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Application.UnitTests.Fakes;

namespace Application.UnitTests.Services;

public sealed class UserServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeTopicRepository _topics = new();
    private readonly FakeUserTopicRepository _registrations;
    private readonly UserService _sut;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

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

        var result = await _sut.GetAllAsync(Ct);

        Assert.Equal(["Ana", "Marko"], result.Select(u => u.FirstName));
    }

    [Fact]
    public async Task RegisterUserAsync_CopiesFieldsAndSaves()
    {
        var dto = new UserCreateDto { FirstName = "Ana", LastName = "Anić", Email = "ana@example.com" };

        var user = await _sut.RegisterUserAsync(dto, Ct);

        Assert.Single(_users.Items);
        Assert.Equal(dto.FirstName, user.FirstName);
        Assert.Equal(dto.LastName, user.LastName);
        Assert.Equal(dto.Email, user.Email);
        Assert.Equal(1, _users.SaveChangesCalls);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsIt()
    {
        var user = _users.Seed("Ana");

        var result = await _sut.GetByIdAsync(user.Id, Ct);

        Assert.Equal(new UserResponse(user.Id, "Ana", user.LastName, user.Email), result);
    }

    [Fact]
    public async Task GetByIdAsync_MissingUser_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(42, Ct));
    }

    [Fact]
    public async Task UpdateAsync_ExistingUser_UpdatesFieldsAndSaves()
    {
        var user = _users.Seed("Ana");
        var dto = new UserCreateDto { FirstName = "Ana", LastName = "Petrović", Email = "ana.p@example.com" };

        await _sut.UpdateAsync(user.Id, dto, Ct);

        Assert.Equal("Petrović", user.LastName);
        Assert.Equal("ana.p@example.com", user.Email);
        Assert.Equal(1, _users.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_MissingUser_ThrowsAndDoesNotSave()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(42, new UserCreateDto(), Ct));

        Assert.Equal(0, _users.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterUserAsync_NormalizesEmail()
    {
        var dto = new UserCreateDto { FirstName = "Ana", LastName = "Anić", Email = "  Ana.Anic@Example.COM " };

        var user = await _sut.RegisterUserAsync(dto, Ct);

        Assert.Equal("ana.anic@example.com", user.Email);
        Assert.Equal("ana.anic@example.com", Assert.Single(_users.Items).Email);
    }

    [Fact]
    public async Task RegisterUserAsync_EmailTakenInDifferentCase_ThrowsAndDoesNotSave()
    {
        _users.Seed("Ana").Email = "ana@example.com";
        var dto = new UserCreateDto { FirstName = "Ana", LastName = "Druga", Email = "ANA@example.com" };

        await Assert.ThrowsAsync<ConflictException>(() => _sut.RegisterUserAsync(dto, Ct));

        Assert.Single(_users.Items);
        Assert.Equal(0, _users.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_KeepingOwnEmail_Succeeds()
    {
        var user = _users.Seed("Ana");
        user.Email = "ana@example.com";
        var dto = new UserCreateDto { FirstName = "Ana", LastName = "Nova", Email = "Ana@Example.com" };

        await _sut.UpdateAsync(user.Id, dto, Ct);

        Assert.Equal("Nova", user.LastName);
        Assert.Equal("ana@example.com", user.Email);
        Assert.Equal(1, _users.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_EmailOfAnotherUser_ThrowsAndDoesNotChange()
    {
        _users.Seed("Ana").Email = "ana@example.com";
        var marko = _users.Seed("Marko");
        marko.Email = "marko@example.com";
        var dto = new UserCreateDto { FirstName = "Marko", LastName = "Test", Email = "ana@example.com" };

        await Assert.ThrowsAsync<ConflictException>(() => _sut.UpdateAsync(marko.Id, dto, Ct));

        Assert.Equal("marko@example.com", marko.Email);
        Assert.Equal(0, _users.SaveChangesCalls);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_RemovesIt()
    {
        var user = _users.Seed();

        await _sut.DeleteAsync(user.Id, Ct);

        Assert.Empty(_users.Items);
        Assert.Equal(1, _users.SaveChangesCalls);
    }

    [Fact]
    public async Task DeleteAsync_MissingUser_ThrowsAndDoesNotSave()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(42, Ct));

        Assert.Equal(0, _users.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterToTopicAsync_ValidUserAndTopic_AddsRegistration()
    {
        var user = _users.Seed();
        var topic = _topics.Seed(conferenceId: 1);

        await _sut.RegisterToTopicAsync(new UserTopicDto { UserId = user.Id, TopicId = topic.Id }, Ct);

        var registration = Assert.Single(_registrations.Items);
        Assert.Equal(user.Id, registration.UserId);
        Assert.Equal(topic.Id, registration.TopicId);
        Assert.Equal(1, _registrations.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterToTopicAsync_MissingUser_Throws()
    {
        var topic = _topics.Seed(conferenceId: 1);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.RegisterToTopicAsync(new UserTopicDto { UserId = 42, TopicId = topic.Id }, Ct));

        Assert.Empty(_registrations.Items);
    }

    [Fact]
    public async Task RegisterToTopicAsync_MissingTopic_Throws()
    {
        var user = _users.Seed();

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.RegisterToTopicAsync(new UserTopicDto { UserId = user.Id, TopicId = 42 }, Ct));

        Assert.Empty(_registrations.Items);
    }

    [Fact]
    public async Task RegisterToTopicAsync_AlreadyRegistered_ThrowsAndDoesNotDuplicate()
    {
        var user = _users.Seed();
        var topic = _topics.Seed(conferenceId: 1);
        _registrations.Seed(user.Id, topic.Id);

        await Assert.ThrowsAsync<ConflictException>(
            () => _sut.RegisterToTopicAsync(new UserTopicDto { UserId = user.Id, TopicId = topic.Id }, Ct));

        Assert.Single(_registrations.Items);
        Assert.Equal(0, _registrations.SaveChangesCalls);
    }

    [Fact]
    public async Task UnregisterFromTopicAsync_ExistingRegistration_RemovesIt()
    {
        var user = _users.Seed();
        var topic = _topics.Seed(conferenceId: 1);
        _registrations.Seed(user.Id, topic.Id);

        await _sut.UnregisterFromTopicAsync(new UserTopicDto { UserId = user.Id, TopicId = topic.Id }, Ct);

        Assert.Empty(_registrations.Items);
        Assert.Equal(1, _registrations.SaveChangesCalls);
    }

    [Fact]
    public async Task UnregisterFromTopicAsync_MissingRegistration_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UnregisterFromTopicAsync(new UserTopicDto { UserId = 1, TopicId = 1 }, Ct));

        Assert.Equal(0, _registrations.SaveChangesCalls);
    }

    [Fact]
    public async Task GetUserTopicsAsync_ReturnsRegisteredTopicsOnly()
    {
        var user = _users.Seed();
        var registered = _topics.Seed(conferenceId: 1, "Registered");
        _topics.Seed(conferenceId: 1, "Other");
        _registrations.Seed(user.Id, registered.Id);

        var result = await _sut.GetUserTopicsAsync(user.Id, Ct);

        Assert.Equal([registered.Id], result.Select(t => t.Id));
    }

    [Fact]
    public async Task GetUserTopicsAsync_ExistingUserWithoutRegistrations_ReturnsEmpty()
    {
        var user = _users.Seed();

        var result = await _sut.GetUserTopicsAsync(user.Id, Ct);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserTopicsAsync_MissingUser_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetUserTopicsAsync(42, Ct));
    }

    [Fact]
    public async Task GetUsersByTopicAsync_ReturnsRegisteredUsersOnly()
    {
        var topic = _topics.Seed(conferenceId: 1);
        var registered = _users.Seed("Ana");
        _users.Seed("Marko");
        _registrations.Seed(registered.Id, topic.Id);

        var result = await _sut.GetUsersByTopicAsync(topic.Id, Ct);

        Assert.Equal([registered.Id], result.Select(u => u.Id));
    }

    [Fact]
    public async Task GetUsersByTopicAsync_MissingTopic_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetUsersByTopicAsync(42, Ct));
    }
}
