using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using GameBackend.API.Data;
using GameBackend.API.DTO;
using GameBackend.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GameBackend.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public LoginController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // Kullanıcı yoksa kayıt + token
        // Varsa şifre doğrula + token
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == req.Username);

            // Yeni kullanıcı -> kayıt
            if (user == null)
            {
                var hashed = BCrypt.Net.BCrypt.HashPassword(req.Password);
                user = new User { Username = req.Username, PasswordHash = hashed };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }
            else
            {
                // var olan kullanıcı -> password doğrulama
                if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                    return BadRequest("Invalid credentials");
            }

            var token = GenerateJwt(user);
            return Ok(new { token });
        }

        private string GenerateJwt(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiresInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
