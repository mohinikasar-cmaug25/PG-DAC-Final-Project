using InnovateConnect.API.Data;
using InnovateConnect.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InnovateConnect.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternshipsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InternshipsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Internship>>> GetInternships()
        {
            return await _context.Internships.Include(i => i.Company).ToListAsync();
        }

        private int? GetUserId()
        {
            // Try all possible claim types for the user ID
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier) 
                             ?? User.Claims.FirstOrDefault(c => c.Type == "sub")
                             ?? User.Claims.FirstOrDefault(c => c.Type == "id")
                             ?? User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier"));
            
            if (userIdClaim == null) return null;
            if (int.TryParse(userIdClaim.Value, out int id)) return id;
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<Internship>> PostInternship(Internship internship)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User identification claim (sub/id/nameidentifier) not found in token.");

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null) return BadRequest($"Company profile not found for User ID: {userId}. Please ensure you are registered as a company.");

            internship.CompanyId = company.Id;
            internship.Company = null;
            internship.PostedDate = DateTime.UtcNow;

            _context.Internships.Add(internship);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInternships", new { id = internship.Id }, internship);
        }

        [HttpGet("my-internships")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<IEnumerable<Internship>>> GetMyInternships()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("User identification claim not found in token.");

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);
            
            if(company == null) return BadRequest($"Company profile not found for User ID: {userId}.");

            return await _context.Internships
                .Where(i => i.CompanyId == company.Id)
                .Include(i => i.Company)
                .ToListAsync();
        }

        [HttpGet("{id}/applicants")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetInternshipApplicants(int id)
        {
             var userId = GetUserId();
             if (userId == null) return Unauthorized("User identification claim not found in token.");

             var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);

             if (company == null) return BadRequest($"Company profile not found for User ID: {userId}.");

             var internship = await _context.Internships.FindAsync(id);
             if (internship == null) return NotFound("Internship not found.");

             if (internship.CompanyId != company.Id) return Forbid("You do not own this internship.");

             // Assuming there is an Application model. If not, I need to check Models again.
             // Based on previous `list_dir` of Models, `Application.cs` exists.
             var applications = await _context.Applications
                 .Include(a => a.Student)
                 .Where(a => a.InternshipId == id)
                 .Select(a => new 
                 {
                     a.Id,
                     ApplicationDate = a.AppliedDate,
                     Status = a.Status,
                     Student = new 
                     {
                         a.Student.Id,
                         a.Student.FullName,
                         a.Student.University,
                         a.Student.GitHubLink,
                         a.Student.LeetCodeLink,
                         a.Student.Bio,
                         a.Student.Skills,
                         ResumeFileName = a.Student.Resume != null ? a.Student.Resume.ResumeFileName : null
                     }
                 })
                 .ToListAsync();

             return Ok(applications);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> PutInternship(int id, Internship internship)
        {
            if (id != internship.Id) return BadRequest();

            var userId = GetUserId();
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);
            if (company == null) return BadRequest("Company profile not found.");

            var existingInternship = await _context.Internships.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            if (existingInternship == null) return NotFound();
            if (existingInternship.CompanyId != company.Id) return Forbid("You do not own this internship.");

            internship.CompanyId = company.Id;
            internship.Company = null;
            internship.PostedDate = existingInternship.PostedDate; // Preserve original post date

            _context.Entry(internship).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InternshipExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> DeleteInternship(int id)
        {
            var userId = GetUserId();
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);
            if (company == null) return BadRequest("Company profile not found.");

            var internship = await _context.Internships.FindAsync(id);
            if (internship == null) return NotFound();

            if (internship.CompanyId != company.Id) return Forbid("You do not own this internship.");

            _context.Internships.Remove(internship);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InternshipExists(int id)
        {
            return _context.Internships.Any(e => e.Id == id);
        }
    }
}
