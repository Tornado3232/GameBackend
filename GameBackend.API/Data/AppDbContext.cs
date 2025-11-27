using GameBackend.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

        public DbSet<User> Users => Set<User>();
    }
}
