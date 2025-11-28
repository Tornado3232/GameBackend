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
        public async Task<IActionResult> AddEvent([FromBody] EventDto req)
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
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var events = await _db.Events
                .OrderByDescending(x => x.TsUtc)
                .Take(100)
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<Dictionary<string, int>>> GetStats()
        {
            var stats = await _db.Events
            .GroupBy(e => e.EventType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

            return Ok(stats);
        }
    }
}
