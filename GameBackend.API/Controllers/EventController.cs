using GameBackend.API.Data;
using GameBackend.API.DTO;
using GameBackend.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GameBackend.API.Controllers
{
    [Authorize]
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
            if (!Request.Headers.TryGetValue("Idempotency-Key", out var headerKey))
            {
                return BadRequest("Missing Idempotency-Key header.");
            }

            var key = headerKey.ToString();

            var existingRecord = await _db.IdempotencyRecords
                .FirstOrDefaultAsync(x => x.Key == key);

            if (existingRecord != null)
            {
                return Content(existingRecord.ResponseBody, "application/json");
            }

            var eventArbitrary = new Event
            {
                UserId = req.UserId,
                EventType = req.EventType,
                Meta = req.Meta,
                TsUtc = req.TsUtc
            };

            _db.Events.Add(eventArbitrary);
            await _db.SaveChangesAsync();

            var responseJson = JsonSerializer.Serialize(eventArbitrary);

            var record = new IdempotencyRecord
            {
                Key = key,
                ResponseBody = responseJson,
            };

            _db.IdempotencyRecords.Add(record);
            await _db.SaveChangesAsync();

            return Ok(eventArbitrary);
        }


        [HttpGet("events")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest("page and pageSize must be positive.");

            var query = _db.Events.OrderByDescending(x => x.TsUtc);

            var totalCount = await query.CountAsync();

            var events = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                page,
                pageSize,
                totalCount,
                data = events
            };

            return Ok(result);
        }


        [HttpGet("stats")]
        public async Task<ActionResult<Dictionary<string, int>>> GetStats([FromQuery] int? userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = _db.Events.AsQueryable();

            if (userId.HasValue)
                query = query.Where(e => e.UserId == userId.Value);

            if (startDate.HasValue)
                query = query.Where(e => e.TsUtc >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.TsUtc <= endDate.Value);

            var stats = await query
                .GroupBy(e => e.EventType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            return Ok(stats);
        }

    }
}
