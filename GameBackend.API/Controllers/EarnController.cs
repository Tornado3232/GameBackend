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
    public class EarnController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EarnController(AppDbContext db)
        {
            _db = db;
        }

        //[HttpPost("earn")]
        //public async Task<IActionResult> Earn([FromBody] EarnDto req)
        //{
        //    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId);
        //    if (user == null)
        //        return NotFound("User not found");

        //    user.Balance += req.Amount;

        //    var eventModel = new Event
        //    {
        //        UserId = req.UserId,
        //        EventType = "earn",
        //        Meta = req.Reason,
        //        TsUtc = DateTime.UtcNow
        //    };

        //    _db.Events.Add(eventModel);
        //    await _db.SaveChangesAsync();

        //    return Ok(new { user.Id, user.Username, user.Balance });
        //}

        [HttpPost("earn")]
        public async Task<IActionResult> Earn([FromBody] EarnDto req)
        {
            if (!Request.Headers.TryGetValue("Idempotency-Key", out var headerKey))
                return BadRequest("Missing Idempotency-Key header.");

            var  key = headerKey.ToString();

            var existingRecord = await _db.IdempotencyRecords
                .FirstOrDefaultAsync(x => x.Key == key);

            if (existingRecord != null)
            {
                return Content(existingRecord.ResponseBody, "application/json");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId);
            if (user == null)
                return NotFound("User not found");

            user.Balance += req.Amount;

            var eventModel = new Event
            {
                UserId = req.UserId,
                EventType = "earn",
                Meta = req.Reason,
                TsUtc = DateTime.UtcNow
            };

            _db.Events.Add(eventModel);

            await _db.SaveChangesAsync();

            var responseObject = new
            {
                user.Id,
                user.Username,
                user.Balance
            };

            var responseJson = JsonSerializer.Serialize(responseObject);

            var record = new IdempotencyRecord
            {
                Key = key,
                ResponseBody = responseJson
            };

            _db.IdempotencyRecords.Add(record);
            await _db.SaveChangesAsync();

            return Ok(responseObject);
        }


    }
}
