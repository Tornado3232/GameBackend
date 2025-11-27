using GameBackend.API.Data;
using GameBackend.API.DTO;
using GameBackend.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            return Ok();
        }

        [HttpPut("updateBalance")]
        public async Task<IActionResult> UpdateBalance(UserDto req)
        {
            return Ok();
        }
    }
}
