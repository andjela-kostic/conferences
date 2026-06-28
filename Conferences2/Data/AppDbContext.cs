using Conferences2.Models;
using Microsoft.EntityFrameworkCore;

namespace Conferences2.Data;

public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}
    
    public DbSet<Conference> Conferences { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserTopic> UserTopics { get; set; }
}