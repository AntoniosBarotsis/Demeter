using Domain.Models;
using Domain.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class DemeterDbContext : DbContext
    {
        public DemeterDbContext(DbContextOptions<DemeterDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Convert enum to string instead of an int
            modelBuilder
                .Entity<User>()
                .Property(u => u.UserType)
                .HasConversion<string>();

            modelBuilder
                .Entity<User>()
                .HasMany(u => u.PastOrders);

            modelBuilder
                .Entity<Order>()
                .HasMany(o => o.Items);
            
        }
    }
}