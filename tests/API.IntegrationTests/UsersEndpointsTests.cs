using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests;

public sealed class UsersEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Post_CreatesUser_VisibleInGetAll()
    {
        var user = await _client.CreateUserAsync(Ct);

        var all = await _client.GetFromJsonAsync<List<UserResponse>>("/api/users", Ct);

        Assert.Equal(user.Email, Assert.Single(all!, u => u.Id == user.Id).Email);
    }

    [Fact]
    public async Task Post_ReturnsLocationOfNewUser()
    {
        var response = await _client.PostAsJsonAsync("/api/users", new
        {
            FirstName = "Grace",
            LastName = "Hopper",
            Email = $"grace.{Guid.NewGuid():N}@example.com",
        }, Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<UserResponse>(Ct);
        var fetched = await _client.GetFromJsonAsync<UserResponse>(response.Headers.Location, Ct);
        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task GetById_MissingUser_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync("/api/users/999999", Ct);

        await ProblemAssert.HasStatusAsync(response, HttpStatusCode.NotFound, Ct);
    }

    [Fact]
    public async Task Put_ExistingUser_UpdatesIt()
    {
        var user = await _client.CreateUserAsync(Ct);

        var response = await _client.PutAsJsonAsync($"/api/users/{user.Id}", new
        {
            FirstName = "Ada",
            LastName = "King",
            user.Email,
        }, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updated = await _client.GetFromJsonAsync<UserResponse>($"/api/users/{user.Id}", Ct);
        Assert.Equal("King", updated!.LastName);
    }

    [Fact]
    public async Task Put_MissingUser_Returns404ProblemDetails()
    {
        var response = await _client.PutAsJsonAsync("/api/users/999999", new
        {
            FirstName = "X",
            LastName = "Y",
            Email = "x@example.com",
        }, Ct);

        await ProblemAssert.HasStatusAsync(response, HttpStatusCode.NotFound, Ct);
    }

    [Fact]
    public async Task Delete_UserWithRegistrations_RemovesUserAndRegistrations()
    {
        var conference = await _client.CreateConferenceAsync(Ct);
        var topic = await _client.CreateTopicAsync(conference.Id, Ct);
        var user = await _client.CreateUserAsync(Ct);
        await _client.PostAsJsonAsync("/api/users/register-topic", new { UserId = user.Id, TopicId = topic.Id }, Ct);

        var response = await _client.DeleteAsync($"/api/users/{user.Id}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/users/{user.Id}", Ct)).StatusCode);
        var topicUsers = await _client.GetFromJsonAsync<List<UserResponse>>($"/api/users/topic/{topic.Id}", Ct);
        Assert.Empty(topicUsers!);
    }

    [Fact]
    public async Task Delete_MissingUser_Returns404ProblemDetails()
    {
        var response = await _client.DeleteAsync("/api/users/999999", Ct);

        await ProblemAssert.HasStatusAsync(response, HttpStatusCode.NotFound, Ct);
    }

    [Fact]
    public async Task RegisterAndUnregister_Topic_FullFlow()
    {
        var conference = await _client.CreateConferenceAsync(Ct);
        var topic = await _client.CreateTopicAsync(conference.Id, Ct);
        var user = await _client.CreateUserAsync(Ct);
        var registration = new { UserId = user.Id, TopicId = topic.Id };

        var register = await _client.PostAsJsonAsync("/api/users/register-topic", registration, Ct);
        Assert.Equal(HttpStatusCode.NoContent, register.StatusCode);

        var userTopics = await _client.GetFromJsonAsync<List<TopicResponse>>($"/api/users/{user.Id}/topics", Ct);
        Assert.Equal([topic.Id], userTopics!.Select(t => t.Id));

        var topicUsers = await _client.GetFromJsonAsync<List<UserResponse>>($"/api/users/topic/{topic.Id}", Ct);
        Assert.Equal([user.Id], topicUsers!.Select(u => u.Id));

        var unregister = await _client.PostAsJsonAsync("/api/users/unregister-topic", registration, Ct);
        Assert.Equal(HttpStatusCode.NoContent, unregister.StatusCode);

        userTopics = await _client.GetFromJsonAsync<List<TopicResponse>>($"/api/users/{user.Id}/topics", Ct);
        Assert.Empty(userTopics!);
    }

    [Fact]
    public async Task RegisterTopic_Twice_Returns409ProblemDetails()
    {
        var conference = await _client.CreateConferenceAsync(Ct);
        var topic = await _client.CreateTopicAsync(conference.Id, Ct);
        var user = await _client.CreateUserAsync(Ct);
        var registration = new { UserId = user.Id, TopicId = topic.Id };
        await _client.PostAsJsonAsync("/api/users/register-topic", registration, Ct);

        var second = await _client.PostAsJsonAsync("/api/users/register-topic", registration, Ct);

        await ProblemAssert.HasStatusAsync(second, HttpStatusCode.Conflict, Ct);
    }

    [Fact]
    public async Task RegisterTopic_MissingUserOrTopic_Returns404ProblemDetails()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/users/register-topic", new { UserId = 999999, TopicId = 999999 }, Ct);

        await ProblemAssert.HasStatusAsync(response, HttpStatusCode.NotFound, Ct);
    }

    [Fact]
    public async Task UnregisterTopic_NotRegistered_Returns404()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/users/unregister-topic", new { UserId = 999999, TopicId = 999999 }, Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserTopics_MissingUser_Returns404()
    {
        var response = await _client.GetAsync("/api/users/999999/topics", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersByTopic_MissingTopic_Returns404()
    {
        var response = await _client.GetAsync("/api/users/topic/999999", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
