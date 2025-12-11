using Microsoft.EntityFrameworkCore;
using SensoreApp.Models; // Links to the Models file we just created

namespace SensoreApp.Data // Defines the SensoreApp.Data namespace
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SensorData> SensorData { get; set; }
        public DbSet<UserFeedback> UserFeedback { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorData>()
                .HasIndex(s => new { s.UserId, s.Timestamp });

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Role);
                
            base.OnModelCreating(modelBuilder);
        }
    }
}