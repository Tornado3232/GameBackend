using GameBackend.API.Controllers;
using GameBackend.API.Data;
using GameBackend.API.DTO;
using GameBackend.API.Models;
using GameBackend.API.Test.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.API.Test
{
    public class EarnTest
    {
        [Fact]
        public async Task Earn_Should_Success()
        {
            var db = InMemoryDbService.GetDb();
            var controller = new EarnController(db);

            // Create test user
            var user = new User
            {
                Id = 1,
                Username = "test",
                PasswordHash = new byte[1],
                PasswordSalt = new byte[1],
                Balance = 0
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Request.Headers["Idempotency-Key"] = "key-123";

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };
            EarnDto req = new(user.Id, 50, "Bonus");

            // Act
            var result = await controller.Earn(req);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);

            var resultObject = ok.Value!;

            var dict = resultObject
                .GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(resultObject));

            Assert.Equal(user.Id, (int)dict["Id"]);
            Assert.Equal(user.Username, (string)dict["Username"]);
            Assert.Equal(50, (int)dict["Balance"]);

            var updated = await db.Users.FirstAsync(u => u.Id == user.Id);
            Assert.Equal(50, updated.Balance);
        }

        [Fact]
        public async Task Earn_Should_Fail_Missing_Idempotency_Key()
        {
            var db = InMemoryDbService.GetDb();
            var controller = new EarnController(db);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            EarnDto req = new(1, 10, "test");

            // Act
            var result = await controller.Earn(req);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing Idempotency-Key header.", bad.Value);
        }

        [Fact]
        public async Task Earn_Should_Succes_With_Cache()
        {
            var db = InMemoryDbService.GetDb();
            var controller = new EarnController(db);

            db.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = "key-abc",
                ResponseBody = "{\"Id\":1,\"Username\":\"test\",\"Balance\":100}"
            });

            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Request.Headers["Idempotency-Key"] = "key-abc";

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            EarnDto req = new(1, 50, "bonus");

            // Act
            var result = await controller.Earn(req);

            // Assert
            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", content.ContentType);
            Assert.Equal("{\"Id\":1,\"Username\":\"test\",\"Balance\":100}", content.Content);
        }

        [Fact]
        public async Task Earn_Should_Fail_User_Not_Found()
        {
            var db = InMemoryDbService.GetDb();
            var controller = new EarnController(db);

            var context = new DefaultHttpContext();
            context.Request.Headers["Idempotency-Key"] = "key-xyz";

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            EarnDto req = new(999, 10, "test");

            // Act
            var result = await controller.Earn(req);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found", notFound.Value);
        }
    }
}
