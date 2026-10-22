using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Application.UnitTests.Fakes;

namespace Application.UnitTests.Services;

public sealed class TopicServiceTests
{
    private readonly FakeTopicRepository _topics = new();
    private readonly FakeConferenceRepository _conferences = new();
    private readonly TopicService _sut;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public TopicServiceTests()
    {
        _sut = new TopicService(_topics, _conferences);
    }

    private static TopicCreateDto Dto(int conferenceId) => new()
    {
        Name = "Minimal APIs",
        Description = "Deep dive",
        ConferenceId = conferenceId,
    };

    [Fact]
    public async Task GetAllAsync_ReturnsAllTopics()
    {
        _topics.Seed(conferenceId: 1);
        _topics.Seed(conferenceId: 2);

        var result = await _sut.GetAllAsync(Ct);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTopic_ReturnsIt()
    {
        var topic = _topics.Seed(conferenceId: 1, "Minimal APIs");

        var result = await _sut.GetByIdAsync(topic.Id, Ct);

        Assert.Equal(new TopicResponse(topic.Id, "Minimal APIs", topic.Description, 1), result);
    }

    [Fact]
    public async Task GetByIdAsync_MissingTopic_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(42, Ct));
    }

    [Fact]
    public async Task GetByConferenceAsync_ReturnsOnlyTopicsOfThatConference()
    {
        var conference = _conferences.Seed();
        var other = _conferences.Seed();
        var mine = _topics.Seed(conference.Id);
        _topics.Seed(other.Id);

        var result = await _sut.GetByConferenceAsync(conference.Id, Ct);

        Assert.Equal([mine.Id], result.Select(t => t.Id));
    }

    [Fact]
    public async Task GetByConferenceAsync_ExistingConferenceWithoutTopics_ReturnsEmpty()
    {
        var conference = _conferences.Seed();

        var result = await _sut.GetByConferenceAsync(conference.Id, Ct);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByConferenceAsync_MissingConference_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByConferenceAsync(42, Ct));
    }

    [Fact]
    public async Task CreateAsync_ExistingConference_AddsTopicAndSaves()
    {
        var conference = _conferences.Seed();
        var dto = Dto(conference.Id);

        var topic = await _sut.CreateAsync(dto, Ct);

        Assert.Single(_topics.Items);
        Assert.Equal(dto.Name, topic.Name);
        Assert.Equal(dto.Description, topic.Description);
        Assert.Equal(conference.Id, topic.ConferenceId);
        Assert.Equal(1, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsync_MissingConference_ThrowsAndDoesNotAdd()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(Dto(42), Ct));

        Assert.Empty(_topics.Items);
        Assert.Equal(0, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndCanMoveTopicToAnotherConference()
    {
        var from = _conferences.Seed();
        var to = _conferences.Seed();
        var topic = _topics.Seed(from.Id, "Old");
        var dto = Dto(to.Id);

        await _sut.UpdateAsync(topic.Id, dto, Ct);

        Assert.Equal(dto.Name, topic.Name);
        Assert.Equal(dto.Description, topic.Description);
        Assert.Equal(to.Id, topic.ConferenceId);
        Assert.Equal(1, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_MissingTopic_Throws()
    {
        var conference = _conferences.Seed();

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(42, Dto(conference.Id), Ct));

        Assert.Equal(0, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_MissingConference_ThrowsAndLeavesTopicUnchanged()
    {
        var conference = _conferences.Seed();
        var topic = _topics.Seed(conference.Id, "Original");

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(topic.Id, Dto(42), Ct));

        Assert.Equal("Original", topic.Name);
        Assert.Equal(conference.Id, topic.ConferenceId);
        Assert.Equal(0, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTopic()
    {
        var topic = _topics.Seed(conferenceId: 1);

        await _sut.DeleteAsync(topic.Id, Ct);

        Assert.Empty(_topics.Items);
        Assert.Equal(1, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task DeleteAsync_MissingTopic_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(42, Ct));

        Assert.Equal(0, _topics.SaveChangesCalls);
    }
}
