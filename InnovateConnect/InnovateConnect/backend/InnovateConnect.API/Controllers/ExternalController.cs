using InnovateConnect.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace InnovateConnect.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalController : ControllerBase
    {
        private readonly ILeetCodeService _leetCodeService;
        private readonly InnovateConnect.API.Data.AppDbContext _context;

        public ExternalController(ILeetCodeService leetCodeService, InnovateConnect.API.Data.AppDbContext context)
        {
            _leetCodeService = leetCodeService;
            _context = context;
        }

        [HttpPost("contact")]
        public async Task<IActionResult> PostContactMessage(InnovateConnect.API.Models.ContactMessage message)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            message.CreatedAt = DateTime.UtcNow;
            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Message sent successfully!" });
        }

        [HttpGet("leetcode/{username}")]
        public async Task<IActionResult> GetLeetCodeStats(string username)
        {
            var stats = await _leetCodeService.GetStatsAsync(username);
            
            if (stats.Status == "error")
            {
                 // We return 200 even for error status from upstream to let frontend handle it gracefully, 
                 // or we could return 400/404. Let's return 404 if user not found, 400 for other errors if needed.
                 // For now, mirroring the DTO status is fine, but standard HTTP is better.
                 if (stats.Message == "User not found") return NotFound(new { message = stats.Message });
                 return BadRequest(new { message = stats.Message });
            }

            return Ok(stats);
        }
    }
}
