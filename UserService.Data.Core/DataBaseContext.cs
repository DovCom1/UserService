using Microsoft.EntityFrameworkCore;
using UserService.Model.Entities;

namespace UserService.Data.Core;

public class DataBaseContext(DbContextOptions<DataBaseContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<FriendUser> Friends { get; set; }
    public DbSet<EnemyUser> Enemies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataBaseContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}