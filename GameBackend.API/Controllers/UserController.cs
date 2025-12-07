using GameBackend.API.Data;
using GameBackend.API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UserController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetBalance([FromRoute] int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized("Invalid User");

            return Ok(user.Balance);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateBalance([FromBody] UserDto req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId);
            if (user == null)
                return NotFound("User not found");

            user.Balance += req.Balance;  
            await _db.SaveChangesAsync();

            return Ok(new { user.Id, user.Username, user.Balance });
        }
    }
}
