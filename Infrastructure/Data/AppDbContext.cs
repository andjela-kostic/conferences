using Application.Exceptions;
using Domain.Entities;
using Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Conference> Conferences => Set<Conference>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserTopic> UserTopics => Set<UserTopic>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Services check uniqueness up front; this is the safety net for concurrent requests that both pass that check.
    /// A unique-constraint violation becomes a <see cref="ConflictException"/> (409) instead of a 500.
    /// </summary>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw new ConflictException(UniqueViolationMessage(pg.ConstraintName), ex);
        }
    }

    private static string UniqueViolationMessage(string? constraintName) => constraintName switch
    {
        UserConfiguration.EmailIndex => "A user with this e-mail address already exists.",
        UserTopicConfiguration.UserTopicIndex => "User is already registered for this topic.",
        _ => "The request conflicts with an existing resource.",
    };
}
