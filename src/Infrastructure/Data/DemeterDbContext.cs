using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class DemeterDbContext : DbContext
    {
        public DemeterDbContext(DbContextOptions<DemeterDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}