using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Application.UnitTests.Fakes;

namespace Application.UnitTests.Services;

/// <summary>
/// Log events are part of the operational contract: they are structured (filterable by ID properties),
/// use stable event IDs, and never contain personal data such as e-mail addresses or names.
/// </summary>
public sealed class ServiceLoggingTests
{
    private readonly FakeConferenceRepository _conferences = new();
    private readonly FakeTopicRepository _topics = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeUserTopicRepository _registrations;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ServiceLoggingTests()
    {
        _registrations = new FakeUserTopicRepository(_users, _topics);
    }

    [Fact]
    public async Task CreateConference_LogsInformationWithConferenceId()
    {
        var logger = new FakeLogger<ConferenceService>();
        var sut = new ConferenceService(_conferences, logger);

        var conference = await sut.CreateAsync(new ConferenceCreateDto
        {
            Name = "DotNetConf",
            Description = "d",
            Date = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        }, Ct);

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Information, record.Level);
        Assert.Equal(1001, record.Id.Id);
        Assert.Equal(conference.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), record.GetStructuredStateValue("ConferenceId"));
    }

    [Fact]
    public async Task MissingConference_LogsInformationNotFound()
    {
        var logger = new FakeLogger<ConferenceService>();
        var sut = new ConferenceService(_conferences, logger);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.DeleteAsync(42, Ct));

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Information, record.Level);
        Assert.Equal(1004, record.Id.Id);
        Assert.Equal("42", record.GetStructuredStateValue("ConferenceId"));
    }

    [Fact]
    public async Task CreateTopic_LogsTopicAndConferenceIds()
    {
        var logger = new FakeLogger<TopicService>();
        var sut = new TopicService(_topics, _conferences, logger);
        var conference = _conferences.Seed();

        var topic = await sut.CreateAsync(new TopicCreateDto { Name = "T", Description = "D", ConferenceId = conference.Id }, Ct);

        var record = logger.LatestRecord;
        Assert.Equal(2001, record.Id.Id);
        Assert.Equal(topic.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), record.GetStructuredStateValue("TopicId"));
        Assert.Equal(conference.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), record.GetStructuredStateValue("ConferenceId"));
    }

    [Fact]
    public async Task RegisterUser_LogsUserIdWithoutPersonalData()
    {
        var logger = new FakeLogger<UserService>();
        var sut = new UserService(_users, _topics, _registrations, logger);

        var user = await sut.RegisterUserAsync(new UserCreateDto
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada.lovelace@example.com",
        }, Ct);

        var record = logger.LatestRecord;
        Assert.Equal(3001, record.Id.Id);
        Assert.Equal(user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), record.GetStructuredStateValue("UserId"));
        AssertNoPersonalData(logger, "ada.lovelace@example.com", "Ada", "Lovelace");
    }

    [Fact]
    public async Task EmailTaken_LogsWarningWithoutTheAddress()
    {
        var logger = new FakeLogger<UserService>();
        var sut = new UserService(_users, _topics, _registrations, logger);
        _users.Seed("Ana").Email = "taken@example.com";

        await Assert.ThrowsAsync<ConflictException>(() => sut.RegisterUserAsync(new UserCreateDto
        {
            FirstName = "Zorana",
            LastName = "Zoric",
            Email = "Taken@Example.com",
        }, Ct));

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Warning, record.Level);
        Assert.Equal(3005, record.Id.Id);
        AssertNoPersonalData(logger, "taken@example.com", "Zorana", "Zoric");
    }

    [Fact]
    public async Task RegisterToTopic_Twice_LogsWarning()
    {
        var logger = new FakeLogger<UserService>();
        var sut = new UserService(_users, _topics, _registrations, logger);
        var user = _users.Seed();
        var topic = _topics.Seed(conferenceId: 1);
        _registrations.Seed(user.Id, topic.Id);

        await Assert.ThrowsAsync<ConflictException>(
            () => sut.RegisterToTopicAsync(new UserTopicDto { UserId = user.Id, TopicId = topic.Id }, Ct));

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Warning, record.Level);
        Assert.Equal(4003, record.Id.Id);
    }

    [Fact]
    public async Task RegisterToTopic_MissingTopic_LogsWhichSideIsMissing()
    {
        var logger = new FakeLogger<UserService>();
        var sut = new UserService(_users, _topics, _registrations, logger);
        var user = _users.Seed();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.RegisterToTopicAsync(new UserTopicDto { UserId = user.Id, TopicId = 99 }, Ct));

        var record = logger.LatestRecord;
        Assert.Equal(4005, record.Id.Id);
        Assert.Equal("True", record.GetStructuredStateValue("UserExists"));
        Assert.Equal("False", record.GetStructuredStateValue("TopicExists"));
    }

    private static void AssertNoPersonalData(FakeLogger logger, params string[] values)
    {
        foreach (var record in logger.Collector.GetSnapshot())
        {
            foreach (var value in values)
            {
                Assert.DoesNotContain(value, record.Message, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(record.StructuredState ?? [], kv => string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}


