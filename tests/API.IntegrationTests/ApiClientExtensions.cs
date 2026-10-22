using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests;

internal static class ApiClientExtensions
{
    public static async Task<ConferenceResponse> CreateConferenceAsync(this HttpClient client, CancellationToken ct)
    {
        var response = await client.PostAsJsonAsync("/api/conferences", new
        {
            Name = "NDC " + Guid.NewGuid().ToString("N")[..8],
            Description = "Integration test conference",
            Date = new DateTime(2027, 6, 1, 9, 0, 0, DateTimeKind.Utc),
        }, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ConferenceResponse>(ct))!;
    }

    public static async Task<TopicResponse> CreateTopicAsync(this HttpClient client, int conferenceId, CancellationToken ct)
    {
        var response = await client.PostAsJsonAsync("/api/topics", new
        {
            Name = "Topic " + Guid.NewGuid().ToString("N")[..8],
            Description = "Integration test topic",
            ConferenceId = conferenceId,
        }, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TopicResponse>(ct))!;
    }

    public static async Task<UserResponse> CreateUserAsync(this HttpClient client, CancellationToken ct)
    {
        var response = await client.PostAsJsonAsync("/api/users", new
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = $"ada.{Guid.NewGuid():N}@example.com",
        }, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<UserResponse>(ct))!;
    }
}

