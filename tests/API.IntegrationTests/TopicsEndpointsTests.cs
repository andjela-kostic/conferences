using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests;

public sealed class TopicsEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Post_ExistingConference_CreatesTopic_VisibleByConference()
    {
        var conference = await _client.CreateConferenceAsync(Ct);
        var topic = await _client.CreateTopicAsync(conference.Id, Ct);

        var topics = await _client.GetFromJsonAsync<List<TopicResponse>>(
            $"/api/topics/conference/{conference.Id}", Ct);

        Assert.Equal([topic.Id], topics!.Select(t => t.Id));
        Assert.Equal(conference.Id, topic.ConferenceId);
    }

    [Fact]
    public async Task Post_MissingConference_Returns404ProblemDetails()
    {
        var response = await _client.PostAsJsonAsync("/api/topics", new
        {
            Name = "Orphan",
            Description = "No conference",
            ConferenceId = 999999,
        }, Ct);

        await ProblemAssert.HasStatusAsync(response, HttpStatusCode.NotFound, Ct);
    }

    [Fact]
    public async Task GetByConference_MissingConference_Returns404()
    {
        var response = await _client.GetAsync("/api/topics/conference/999999", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_MovesTopicToAnotherConference()
    {
        var from = await _client.CreateConferenceAsync(Ct);
        var to = await _client.CreateConferenceAsync(Ct);
        var topic = await _client.CreateTopicAsync(from.Id, Ct);

        var response = await _client.PutAsJsonAsync($"/api/topics/{topic.Id}", new
        {
            topic.Name,
            topic.Description,
            ConferenceId = to.Id,
        }, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var moved = await _client.GetFromJsonAsync<List<TopicResponse>>($"/api/topics/conference/{to.Id}", Ct);
        Assert.Contains(moved!, t => t.Id == topic.Id);
    }

    [Fact]
    public async Task Delete_ExistingTopic_RemovesIt()
    {
        var conference = await _client.CreateConferenceAsync(Ct);
        var topic = await _client.CreateTopicAsync(conference.Id, Ct);

        var response = await _client.DeleteAsync($"/api/topics/{topic.Id}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var topics = await _client.GetFromJsonAsync<List<TopicResponse>>(
            $"/api/topics/conference/{conference.Id}", Ct);
        Assert.Empty(topics!);
    }

    [Fact]
    public async Task Delete_MissingTopic_Returns404()
    {
        var response = await _client.DeleteAsync("/api/topics/999999", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}


