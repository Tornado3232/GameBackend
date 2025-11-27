using GameBackend.API.Data;
using GameBackend.API.Models;
using GameBackend.API.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GameBackend.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<User> _hasher;

        public LoginController(AppDbContext db, IConfiguration config, IPasswordHasher<User> hasher)
        {
            _db = db;
            _config = config;
            _hasher = hasher;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == req.Username);

            if (user == null)
            {
                user = new User
                {
                    Username = req.Username,
                    PasswordHash = _hasher.HashPassword(null!, req.Password)
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }
            else
            {
                var res = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
                if (res == PasswordVerificationResult.Failed)
                    return BadRequest("Invalid credentials");
            }

            var secret = _config["Jwt:Key"];

            string token = JwtGenerator.Generate(user.Username, secret!, 60);

            return Ok(new { token });
        }
    }
}
