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
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EventController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("event")]
        public async Task<IActionResult> AddEvent(EventDto req)
        {
            //Check if works??
            var eventModel = new Event
            {
                UserId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                EventType = req.EventType,
                TsUtc = req.TsUtc,
                Meta = req.Meta
            };

            _db.Events.Add(eventModel);
            await _db.SaveChangesAsync();

            return Ok("Event Created Succesfully");
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(EventDto req)
        {
            //Check if works??
            var eventModel = new Event
            {
                UserId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                EventType = req.EventType,
                TsUtc = req.TsUtc,
                Meta = req.Meta
            };

            _db.Events.Add(eventModel);
            await _db.SaveChangesAsync();

            return Ok("Event Created Succesfully");
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(EventDto req)
        {
            //Check if works??
            var eventModel = new Event
            {
                UserId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                EventType = req.EventType,
                TsUtc = req.TsUtc,
                Meta = req.Meta
            };

            _db.Events.Add(eventModel);
            await _db.SaveChangesAsync();

            return Ok("Event Created Succesfully");
        }
    }
}
