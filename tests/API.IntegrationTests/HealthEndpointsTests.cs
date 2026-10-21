using System.Net;

namespace API.IntegrationTests;

public sealed class HealthEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoint_ReturnsHealthy(string path)
    {
        var response = await _client.GetAsync(path, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task UnknownRoute_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync("/api/does-not-exist", Ct);

        await ProblemAssert.HasStatusAsync(response, HttpStatusCode.NotFound, Ct);
    }
}

