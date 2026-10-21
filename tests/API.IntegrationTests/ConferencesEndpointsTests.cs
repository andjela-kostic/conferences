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


