using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests;

public sealed record ProblemResponse(int? Status, string? Title, string? Detail);

public sealed record ValidationProblemResponse(int? Status, Dictionary<string, string[]> Errors);

internal static class ProblemAssert
{
    /// <summary>Asserts a 400 ValidationProblemDetails with an error for <paramref name="field"/>.</summary>
    public static async Task HasValidationErrorAsync(HttpResponseMessage response, string field, CancellationToken ct)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(ct);
        Assert.NotNull(problem);
        Assert.Contains(problem.Errors.Keys, key => key.Equals(field, StringComparison.OrdinalIgnoreCase));
    }

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

