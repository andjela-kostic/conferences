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

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByConferenceAsync_ReturnsOnlyTopicsOfThatConference()
    {
        var conference = _conferences.Seed();
        var other = _conferences.Seed();
        var mine = _topics.Seed(conference.Id);
        _topics.Seed(other.Id);

        var result = await _sut.GetByConferenceAsync(conference.Id);

        Assert.Equal([mine.Id], result.Select(t => t.Id));
    }

    [Fact]
    public async Task GetByConferenceAsync_ExistingConferenceWithoutTopics_ReturnsEmpty()
    {
        var conference = _conferences.Seed();

        var result = await _sut.GetByConferenceAsync(conference.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByConferenceAsync_MissingConference_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByConferenceAsync(42));
    }

    [Fact]
    public async Task CreateAsync_ExistingConference_AddsTopicAndSaves()
    {
        var conference = _conferences.Seed();
        var dto = Dto(conference.Id);

        var topic = await _sut.CreateAsync(dto);

        Assert.Single(_topics.Items);
        Assert.Equal(dto.Name, topic.Name);
        Assert.Equal(dto.Description, topic.Description);
        Assert.Equal(conference.Id, topic.ConferenceId);
        Assert.Equal(1, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsync_MissingConference_ThrowsAndDoesNotAdd()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(Dto(42)));

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

        var updated = await _sut.UpdateAsync(topic.Id, dto);

        Assert.Equal(dto.Name, updated.Name);
        Assert.Equal(dto.Description, updated.Description);
        Assert.Equal(to.Id, updated.ConferenceId);
        Assert.Equal(1, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_MissingTopic_Throws()
    {
        var conference = _conferences.Seed();

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(42, Dto(conference.Id)));

        Assert.Equal(0, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_MissingConference_ThrowsAndLeavesTopicUnchanged()
    {
        var conference = _conferences.Seed();
        var topic = _topics.Seed(conference.Id, "Original");

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(topic.Id, Dto(42)));

        Assert.Equal("Original", topic.Name);
        Assert.Equal(conference.Id, topic.ConferenceId);
        Assert.Equal(0, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTopic()
    {
        var topic = _topics.Seed(conferenceId: 1);

        await _sut.DeleteAsync(topic.Id);

        Assert.Empty(_topics.Items);
        Assert.Equal(1, _topics.SaveChangesCalls);
    }

    [Fact]
    public async Task DeleteAsync_MissingTopic_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(42));

        Assert.Equal(0, _topics.SaveChangesCalls);
    }
}

