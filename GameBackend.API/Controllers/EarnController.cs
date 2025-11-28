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
    public class EarnController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EarnController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("earn")]
        public async Task<IActionResult> Earn(EarnDto req)
        {
            //Call UpdateBalance Service, Call AddEvent Service
            return Ok();
        }
    }
}
