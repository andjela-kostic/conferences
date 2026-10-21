using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public const string ReadyTag = "ready";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. " +
                "Use user-secrets locally or the ConnectionStrings__DefaultConnection environment variable.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IConferenceRepository, ConferenceRepository>();
        services.AddScoped<ITopicRepository, TopicRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserTopicRepository, UserTopicRepository>();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database", tags: [ReadyTag]);

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}

