using Application.Exceptions;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace API.IntegrationTests;

/// <summary>
/// The unique indexes are the last line of defence when two requests pass the service-level checks at the same time.
/// These tests bypass the services and write directly through <see cref="AppDbContext"/>.
/// </summary>
public sealed class DatabaseConstraintsTests(ApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task DuplicateEmail_ThrowsConflictException()
    {
        var email = $"dup.{Guid.NewGuid():N}@example.com";
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Users.Add(new User { FirstName = "A", LastName = "A", Email = email });
        await db.SaveChangesAsync(Ct);
        db.Users.Add(new User { FirstName = "B", LastName = "B", Email = email });

        var ex = await Assert.ThrowsAsync<ConflictException>(() => db.SaveChangesAsync(Ct));
        Assert.Equal("A user with this e-mail address already exists.", ex.Message);
    }

    [Fact]
    public async Task DuplicateRegistration_ThrowsConflictException()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var conference = new Conference { Name = "C", Description = "D", Date = DateTime.UtcNow };
        var topic = new Topic { Name = "T", Description = "D", Conference = conference };
        var user = new User { FirstName = "U", LastName = "U", Email = $"reg.{Guid.NewGuid():N}@example.com" };
        db.UserTopics.Add(new UserTopic { User = user, Topic = topic });
        await db.SaveChangesAsync(Ct);
        db.UserTopics.Add(new UserTopic { UserId = user.Id, TopicId = topic.Id });

        var ex = await Assert.ThrowsAsync<ConflictException>(() => db.SaveChangesAsync(Ct));
        Assert.Equal("User is already registered for this topic.", ex.Message);
    }
}

