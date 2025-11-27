using BCrypt.Net;
using GameBackend.API.Data;
using GameBackend.API.DTO;
using GameBackend.API.Models;
using GameBackend.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public AuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto req)
        {
            bool exists = await _db.Users.AnyAsync(x => x.Username == req.Username);
            if (exists) return BadRequest("Username is already in use");

            var user = new User
            {
                Username = req.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok("User registered");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == req.Username);
            if (user is null) return BadRequest("Invalid username");

            bool valid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
            if (!valid) return BadRequest("Invalid password");

            string token = _jwt.GenerateToken(user);
            return Ok(new { token });
        }
    }
}
