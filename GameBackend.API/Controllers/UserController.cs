using GameBackend.API.Data;
using GameBackend.API.DTO;
using GameBackend.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GameBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UserController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("getBalance")]
        public async Task<IActionResult> GetBalance(UserDto req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId);
            if (user == null)
                return Unauthorized("Invalid User");

            return Ok(user);
        }

        [HttpPut("updateBalance")]
        public async Task<IActionResult> UpdateBalance(UserDto req)
        {
            return Ok();
        }
    }
}
