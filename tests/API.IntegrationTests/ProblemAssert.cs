using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests;

public sealed record ProblemResponse(int? Status, string? Title, string? Detail);

internal static class ProblemAssert
{
    public static async Task<ProblemResponse> HasStatusAsync(HttpResponseMessage response, HttpStatusCode expected, CancellationToken ct)
    {
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(ct);
        Assert.NotNull(problem);
        Assert.Equal((int)expected, problem.Status);
        return problem;
    }
}

