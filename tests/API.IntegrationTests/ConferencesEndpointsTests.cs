using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests;

public sealed class ConferencesEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Post_CreatesConference_VisibleInGetAll()
    {
        var created = await _client.CreateConferenceAsync(Ct);

        var all = await _client.GetFromJsonAsync<List<ConferenceResponse>>("/api/conferences", Ct);

        var stored = Assert.Single(all!, c => c.Id == created.Id);
        Assert.Equal(created.Name, stored.Name);
        Assert.Equal(created.Date, stored.Date);
    }

    [Fact]
    public async Task Post_ReturnsLocationOfNewConference()
    {
        var response = await _client.PostAsJsonAsync("/api/conferences", new
        {
            Name = "Location test",
            Description = "x",
            Date = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            TopicIds = Array.Empty<int>(),
        }, Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ConferenceResponse>(Ct);
        Assert.EndsWith($"/api/conferences/{created!.Id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetById_ReturnsConferenceWithTopics()
    {
        var conference = await _client.CreateConferenceAsync(Ct);
        var topic = await _client.CreateTopicAsync(conference.Id, Ct);

        var fetched = await _client.GetFromJsonAsync<ConferenceResponse>($"/api/conferences/{conference.Id}", Ct);

        Assert.Equal(conference.Name, fetched!.Name);
        Assert.Equal([topic], fetched.Topics);
    }

    [Fact]
    public async Task GetById_MissingConference_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync("/api/conferences/999999", Ct);

        await ProblemAssert.HasStatusAsync(response, HttpStatusCode.NotFound, Ct);
    }

    [Fact]
    public async Task Response_ContainsOnlyContractFields()
    {
        var conference = await _client.CreateConferenceAsync(Ct);
        await _client.CreateTopicAsync(conference.Id, Ct);

        var json = await _client.GetStringAsync($"/api/conferences/{conference.Id}", Ct);

        Assert.DoesNotContain("\"conference\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("userTopics", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Put_ExistingConference_UpdatesIt()
    {
        var created = await _client.CreateConferenceAsync(Ct);

        var response = await _client.PutAsJsonAsync($"/api/conferences/{created.Id}", new
        {
            Name = "Renamed",
            Description = "Updated",
            Date = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TopicIds = Array.Empty<int>(),
        }, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var all = await _client.GetFromJsonAsync<List<ConferenceResponse>>("/api/conferences", Ct);
        Assert.Equal("Renamed", Assert.Single(all!, c => c.Id == created.Id).Name);
    }

    [Fact]
    public async Task Put_MissingConference_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/api/conferences/999999", new
        {
            Name = "X",
            Description = "X",
            Date = DateTime.UtcNow,
            TopicIds = Array.Empty<int>(),
        }, Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingConference_RemovesIt()
    {
        var created = await _client.CreateConferenceAsync(Ct);

        var response = await _client.DeleteAsync($"/api/conferences/{created.Id}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var all = await _client.GetFromJsonAsync<List<ConferenceResponse>>("/api/conferences", Ct);
        Assert.DoesNotContain(all!, c => c.Id == created.Id);
    }

    [Fact]
    public async Task Delete_MissingConference_Returns404ProblemDetails()
    {
        var response = await _client.DeleteAsync("/api/conferences/999999", Ct);

        var problem = await ProblemAssert.HasStatusAsync(response, HttpStatusCode.NotFound, Ct);
        Assert.Equal("Conference with ID 999999 not found.", problem.Detail);
    }
}
