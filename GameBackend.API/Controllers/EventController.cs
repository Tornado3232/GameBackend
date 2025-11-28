using GameBackend.API.Data;
using GameBackend.API.DTO;
using GameBackend.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Claims;

namespace GameBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EventController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddEvent(EventDto req)
        {
            var eventArbitrary = new Event
            {
                UserId = req.UserId,
                EventType = req.EventType,
                Meta = req.Meta,
                TsUtc = req.TsUtc
            };

            _db.Events.Add(eventArbitrary);
            await _db.SaveChangesAsync();

            return Ok(eventArbitrary);
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(EventDto req)
        {
            //Check if works??
            return Ok();
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(EventDto req)
        {
            //Check if works??
            return Ok();
        }
    }
}
