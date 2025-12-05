using GameBackend.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<AppsFlyer> AppsFlyerPayloads => Set<AppsFlyer>();
        public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    }
}
