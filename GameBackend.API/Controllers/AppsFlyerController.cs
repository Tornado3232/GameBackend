using GameBackend.API.Data;
using GameBackend.API.Helpers;
using GameBackend.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace GameBackend.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppsFlyerController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly AppsFlyerService _afService;

        public AppsFlyerController(AppDbContext db, AppsFlyerService afService)
        {
            _db = db;
            _afService = afService;
        }

        [HttpPost("postback")]
        public async Task<IActionResult> Postback()
        {
            // 1. Read signature header
            if (!Request.Headers.TryGetValue("af-signature", out var signatureHeader))
                return BadRequest("Missing af-signature header");

            string receivedSignature = signatureHeader.ToString();

            // 2. Read raw body (AppsFlyer requires exact body for hashing)
            Request.EnableBuffering();
            Request.Body.Position = 0;

            string rawBody;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
                rawBody = await reader.ReadToEndAsync();

            Request.Body.Position = 0;

            // 3. Validate signature using service
            bool isValid = _afService.VerifySignature(rawBody, receivedSignature);

            if (!isValid)
                return Unauthorized(new { message = "Invalid signature" });

            // 4. Deserialize if needed (optional)
            var bodyObject = System.Text.Json.JsonSerializer.Deserialize<object>(rawBody);

            // 5. Success
            return Ok(new
            {
                message = "Signature valid",
                data = bodyObject
            });
        }
    }
}
