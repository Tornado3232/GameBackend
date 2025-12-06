using BCrypt.Net;
using GameBackend.API.Abstractions;
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
        private readonly IJwtService _jwt;

        public AuthController(AppDbContext db, IJwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto req)
        {
            if (await _db.Users.AnyAsync(u => u.Username == req.Username))
                return BadRequest("User already exists.");

            _jwt.CreatePasswordHash(req.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Username = req.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null)
                return Unauthorized("Invalid Username or Password.");

            if (!_jwt.VerifyPasswordHash(req.Password, user.PasswordHash, user.PasswordSalt))
                return Unauthorized("Invalid Username or Password.");

            var token = _jwt.GenerateToken(user);
            return Ok(new { token });
        }
    }
}
