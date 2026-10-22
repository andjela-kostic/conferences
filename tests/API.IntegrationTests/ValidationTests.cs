using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace API.IntegrationTests;

public sealed class ValidationTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private Task<HttpResponseMessage> PostJsonAsync(string url, string json) =>
        _client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), Ct);

    [Theory]
    [InlineData("""{ "name": "", "description": "d", "date": "2027-06-01T09:00:00Z" }""", "Name")]
    [InlineData("""{ "name": "n", "description": "", "date": "2027-06-01T09:00:00Z" }""", "Description")]
    [InlineData("""{ "name": "n", "description": "d", "date": "2027-06-01T09:00:00" }""", "Date")]
    [InlineData("""{ "name": "n", "description": "d", "date": "2027-06-01T09:00:00+02:00" }""", "Date")]
    [InlineData("""{ "name": "n", "description": "d" }""", "Date")]
    public async Task PostConference_InvalidBody_Returns400(string json, string field)
    {
        var response = await PostJsonAsync("/api/conferences", json);

        await ProblemAssert.HasValidationErrorAsync(response, field, Ct);
    }

    [Fact]
    public async Task PostConference_NameTooLong_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/conferences", new
        {
            Name = new string('x', 201),
            Description = "d",
            Date = new DateTime(2027, 6, 1, 9, 0, 0, DateTimeKind.Utc),
        }, Ct);

        await ProblemAssert.HasValidationErrorAsync(response, "Name", Ct);
    }

    [Fact]
    public async Task PutConference_NonUtcDate_Returns400()
    {
        var conference = await _client.CreateConferenceAsync(Ct);

        var response = await _client.PutAsync(
            $"/api/conferences/{conference.Id}",
            new StringContent("""{ "name": "n", "description": "d", "date": "2027-06-01T09:00:00" }""", Encoding.UTF8, "application/json"),
            Ct);

        await ProblemAssert.HasValidationErrorAsync(response, "Date", Ct);
    }

    [Theory]
    [InlineData("""{ "name": "", "description": "d", "conferenceId": 1 }""", "Name")]
    [InlineData("""{ "name": "n", "description": "d", "conferenceId": 0 }""", "ConferenceId")]
    [InlineData("""{ "name": "n", "description": "d" }""", "ConferenceId")]
    public async Task PostTopic_InvalidBody_Returns400(string json, string field)
    {
        var response = await PostJsonAsync("/api/topics", json);

        await ProblemAssert.HasValidationErrorAsync(response, field, Ct);
    }

    [Theory]
    [InlineData("""{ "firstName": "", "lastName": "L", "email": "a@example.com" }""", "FirstName")]
    [InlineData("""{ "firstName": "F", "lastName": "   ", "email": "a@example.com" }""", "LastName")]
    [InlineData("""{ "firstName": "F", "lastName": "L", "email": "not-an-email" }""", "Email")]
    [InlineData("""{ "firstName": "F", "lastName": "L" }""", "Email")]
    public async Task PostUser_InvalidBody_Returns400(string json, string field)
    {
        var response = await PostJsonAsync("/api/users", json);

        await ProblemAssert.HasValidationErrorAsync(response, field, Ct);
    }

    [Theory]
    [InlineData("/api/users/register-topic", """{ "userId": 0, "topicId": 1 }""", "UserId")]
    [InlineData("/api/users/register-topic", """{ "userId": 1, "topicId": -5 }""", "TopicId")]
    [InlineData("/api/users/unregister-topic", """{ "userId": 0, "topicId": 1 }""", "UserId")]
    public async Task Registration_InvalidIds_Returns400(string url, string json, string field)
    {
        var response = await PostJsonAsync(url, json);

        await ProblemAssert.HasValidationErrorAsync(response, field, Ct);
    }

    [Fact]
    public async Task PostUser_NormalizesEmail()
    {
        var local = $"Mixed.Case.{Guid.NewGuid():N}";

        var response = await _client.PostAsJsonAsync("/api/users", new
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = $"  {local}@Example.COM ",
        }, Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>(Ct);
        Assert.Equal($"{local.ToLowerInvariant()}@example.com", user!.Email);
    }

    [Fact]
    public async Task PostUser_EmailTakenInDifferentCase_Returns409()
    {
        var existing = await _client.CreateUserAsync(Ct);

        var response = await _client.PostAsJsonAsync("/api/users", new
        {
            FirstName = "Other",
            LastName = "Person",
            Email = existing.Email.ToUpperInvariant(),
        }, Ct);

        await ProblemAssert.HasStatusAsync(response, HttpStatusCode.Conflict, Ct);
    }

    [Fact]
    public async Task PutUser_EmailOfAnotherUser_Returns409()
    {
        var first = await _client.CreateUserAsync(Ct);
        var second = await _client.CreateUserAsync(Ct);

        var response = await _client.PutAsJsonAsync($"/api/users/{second.Id}", new
        {
            second.FirstName,
            second.LastName,
            first.Email,
        }, Ct);

        await ProblemAssert.HasStatusAsync(response, HttpStatusCode.Conflict, Ct);
        var unchanged = await _client.GetFromJsonAsync<UserResponse>($"/api/users/{second.Id}", Ct);
        Assert.Equal(second.Email, unchanged!.Email);
    }
}

