using GameBackend.API.Controllers;
using GameBackend.API.DTO;
using GameBackend.API.Models;
using GameBackend.API.Test.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GameBackend.API.Test
{
    public class AuthenticationTest
    {
        [Fact]
        public async Task Register_Should_Success()
        {
            var db = InMemoryDbService.GetDb();
            var jwtMock = MockService.MockJwt();
            var controller = new AuthController(db, jwtMock.Object);

            RegisterDto req = new("newuser", "password123");

            var result = await controller.Register(req);

            Assert.IsType<OkObjectResult>(result);
            var ok = result as OkObjectResult;

            var user = ok!.Value as User;

            Assert.Equal("newuser", user!.Username);
            Assert.NotNull(user.PasswordHash);
            Assert.NotNull(user.PasswordSalt);
        }

        [Fact]
        public async Task Register_Should_Fail()
        {
            var db = InMemoryDbService.GetDb();
            var jwt = new FakeJwtService();

            jwt.CreatePasswordHash("pass", out byte[] passwordHash, out byte[] passwordSalt);

            db.Users.Add(new User { Username = "existing", PasswordHash = passwordHash, PasswordSalt = passwordSalt });
            db.SaveChanges();

            var controller = new AuthController(db, jwt);

            RegisterDto req = new("existing", "pass");

            var result = await controller.Register(req);

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User already exists.", (result as BadRequestObjectResult)!.Value);
        }

        [Fact]
        public async Task Login_Should_Success()
        {
            var db = InMemoryDbService.GetDb();

            db.Users.Add(new User
            {
                Username = "ali",
                PasswordHash = new byte[] { 1, 2, 3 },
                PasswordSalt = new byte[] { 4, 5, 6 }
            });
            db.SaveChanges();

            var jwtMock = MockService.MockJwt();
            var controller = new AuthController(db, jwtMock.Object);

            LoginDto req = new("ali", "correct");

            var result = await controller.Login(req);

            Assert.IsType<OkObjectResult>(result);
            var ok = result as OkObjectResult;

            var json = ok!.Value!.GetType().GetProperty("token")!.GetValue(ok.Value, null);
            Assert.Equal("FAKE_JWT_TOKEN", json);
        }

        [Fact]
        public async Task Login_Should_Fail_Invalid_User()
        {
            var db = InMemoryDbService.GetDb();
            var jwtMock = MockService.MockJwt();
            var controller = new AuthController(db, jwtMock.Object);


            LoginDto req = new("unknown", "password");

            var result = await controller.Login(req);

            Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid Username or Password.",
                (result as UnauthorizedObjectResult)!.Value);
        }

        [Fact]
        public async Task Login_Should_Fail_Incorrect_Password()
        {
            var db = InMemoryDbService.GetDb();

            db.Users.Add(new User
            {
                Username = "test",
                PasswordHash = new byte[] { 1, 2, 3 },
                PasswordSalt = new byte[] { 4, 5, 6 }
            });

            db.SaveChanges();

            var jwtMock = MockService.MockJwt();
            var controller = new AuthController(db, jwtMock.Object);


            LoginDto req = new("tes", "wrong");

            var result = await controller.Login(req);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
