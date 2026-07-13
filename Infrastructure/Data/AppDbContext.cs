using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Conference> Conferences => Set<Conference>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserTopic> UserTopics => Set<UserTopic>();
}