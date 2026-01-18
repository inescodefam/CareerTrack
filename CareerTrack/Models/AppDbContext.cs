using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Goal> Goals { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<GoalProgress> GoalProgress { get; set; }
    }
}