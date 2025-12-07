using GameBackend.API.Data;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.API.Test.Services
{
    public static class InMemoryDbService
    {
        public static AppDbContext GetDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            return new AppDbContext(options);
        }
    }
}
