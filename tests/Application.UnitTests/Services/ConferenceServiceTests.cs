using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Application.UnitTests.Fakes;

namespace Application.UnitTests.Services;

public sealed class ConferenceServiceTests
{
    private readonly FakeConferenceRepository _conferences = new();
    private readonly FakeTopicRepository _topics = new();
    private readonly ConferenceService _sut;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ConferenceServiceTests()
    {
        _sut = new ConferenceService(_conferences, _topics);
    }

    private static ConferenceCreateDto Dto(params int[] topicIds) => new()
    {
        Name = "DotNetConf",
        Description = "Yearly .NET conference",
        Date = new DateTime(2026, 11, 10, 9, 0, 0, DateTimeKind.Utc),
        TopicIds = [.. topicIds],
    };

    [Fact]
    public async Task GetAllAsync_ReturnsAllConferences()
    {
        _conferences.Seed("A");
        _conferences.Seed("B");

        var result = await _sut.GetAllAsync(Ct);

        Assert.Equal(["A", "B"], result.Select(c => c.Name));
    }

    [Fact]
    public async Task CreateAsync_CopiesFieldsAndSaves()
    {
        var dto = Dto();

        var conference = await _sut.CreateAsync(dto, Ct);

        Assert.Single(_conferences.Items);
        Assert.Equal(dto.Name, conference.Name);
        Assert.Equal(dto.Description, conference.Description);
        Assert.Equal(dto.Date, conference.Date);
        Assert.Equal(1, _conferences.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsync_AttachesExistingTopics()
    {
        var first = _topics.Seed(conferenceId: 99);
        var second = _topics.Seed(conferenceId: 99);

        var conference = await _sut.CreateAsync(Dto(first.Id, second.Id), Ct);

        Assert.Equal([first.Id, second.Id], conference.Topics.Select(t => t.Id));
    }

    [Fact]
    public async Task CreateAsync_UnknownTopicIds_AreSilentlyIgnored()
    {
        var existing = _topics.Seed(conferenceId: 99);

        var conference = await _sut.CreateAsync(Dto(existing.Id, 12345), Ct);

        Assert.Equal([existing.Id], conference.Topics.Select(t => t.Id));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingConference_ReturnsItWithTopics()
    {
        var conference = _conferences.Seed("A");
        var topic = _topics.Seed(conference.Id);
        conference.Topics.Add(topic);

        var result = await _sut.GetByIdAsync(conference.Id, Ct);

        Assert.Equal(conference.Id, result.Id);
        Assert.Equal("A", result.Name);
        Assert.Equal([topic.Id], result.Topics.Select(t => t.Id));
    }

    [Fact]
    public async Task GetByIdAsync_MissingConference_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(42, Ct));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndTopics()
    {
        var conference = _conferences.Seed("Old");
        var topic = _topics.Seed(conferenceId: 99);
        var dto = Dto(topic.Id);

        await _sut.UpdateAsync(conference.Id, dto, Ct);

        Assert.Equal(dto.Name, conference.Name);
        Assert.Equal(dto.Description, conference.Description);
        Assert.Equal(dto.Date, conference.Date);
        Assert.Equal([topic.Id], conference.Topics.Select(t => t.Id));
        Assert.Equal(1, _conferences.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_MissingConference_ThrowsAndDoesNotSave()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(42, Dto(), Ct));

        Assert.Equal(0, _conferences.SaveChangesCalls);
    }

    [Fact]
    public async Task DeleteAsync_RemovesConference()
    {
        var conference = _conferences.Seed();

        await _sut.DeleteAsync(conference.Id, Ct);

        Assert.Empty(_conferences.Items);
        Assert.Equal(1, _conferences.SaveChangesCalls);
    }

    [Fact]
    public async Task DeleteAsync_MissingConference_ThrowsAndDoesNotSave()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(42, Ct));

        Assert.Equal(0, _conferences.SaveChangesCalls);
    }
}
