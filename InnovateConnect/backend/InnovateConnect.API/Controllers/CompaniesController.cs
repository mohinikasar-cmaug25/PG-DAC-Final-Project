using InnovateConnect.API.Data;
using InnovateConnect.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InnovateConnect.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompaniesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<Company>> GetProfile()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                             ?? User.FindFirst("sub")
                             ?? User.FindFirst("id");
            
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null) return NotFound("Company profile not found.");

            return company;
        }

        [HttpPut("profile")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> UpdateProfile(DTOs.CompanyUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                             ?? User.FindFirst("sub")
                             ?? User.FindFirst("id");
            
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null) return NotFound("Company profile not found.");

            company.CompanyName = dto.CompanyName;
            company.Location = dto.Location;
            company.Website = dto.Website;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Profile updated successfully", Company = company });
        }
    }
}
