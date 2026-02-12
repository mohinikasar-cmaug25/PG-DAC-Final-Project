using InnovateConnect.API.Data;
using InnovateConnect.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InnovateConnect.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = new
            {
                TotalStudents = await _context.Users.CountAsync(u => u.Role == "Student"),
                TotalCompanies = await _context.Users.CountAsync(u => u.Role == "Company"),
                TotalIdeas = await _context.Ideas.CountAsync(),
                ActiveInternships = await _context.Internships.CountAsync()
            };
            return Ok(stats);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.Role,
                    u.CreatedAt,
                    ProfileName = u.Role == "Student" 
                        ? _context.Students.Where(s => s.UserId == u.Id).Select(s => s.FullName).FirstOrDefault() 
                        : u.Role == "Company" 
                            ? _context.Companies.Where(c => c.UserId == u.Id).Select(c => c.CompanyName).FirstOrDefault() 
                            : "Admin"
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.Role == "Admin") return BadRequest("Cannot delete admin users.");

            // Manual Cascade Delete for Prototype Stability
            if (user.Role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == id);
                if (student != null)
                {
                    // Delete applications
                    var apps = _context.Applications.Where(a => a.StudentId == student.Id);
                    _context.Applications.RemoveRange(apps);
                    
                    // Delete ideas
                    var ideas = _context.Ideas.Where(i => i.StudentId == student.Id);
                    _context.Ideas.RemoveRange(ideas);

                    _context.Students.Remove(student);
                }
            }
            else if (user.Role == "Company")
            {
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == id);
                if (company != null)
                {
                    var internships = await _context.Internships.Where(i => i.CompanyId == company.Id).ToListAsync();
                    foreach (var i in internships)
                    {
                        var apps = _context.Applications.Where(a => a.InternshipId == i.Id);
                        _context.Applications.RemoveRange(apps);
                    }
                    _context.Internships.RemoveRange(internships);
                    _context.Companies.Remove(company);
                }
            }
            
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "User and all related data deleted successfully" });
        }

        [HttpGet("ideas")]
        public async Task<IActionResult> GetIdeas()
        {
            var ideas = await _context.Ideas
                .Include(i => i.Student)
                .Select(i => new {
                    i.Id,
                    i.Title,
                    i.Description,
                    i.TechnologyUsed,
                    i.PostedDate,
                    StudentName = i.Student != null ? i.Student.FullName : "Unknown"
                })
                .ToListAsync();
            return Ok(ideas);
        }

        [HttpDelete("ideas/{id}")]
        public async Task<IActionResult> DeleteIdea(int id)
        {
            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null) return NotFound();
            _context.Ideas.Remove(idea);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Idea deleted successfully" });
        }

        [HttpGet("internships")]
        public async Task<IActionResult> GetInternships()
        {
            var internships = await _context.Internships
                .Include(i => i.Company)
                .Select(i => new {
                    i.Id,
                    i.Title,
                    i.Description,
                    i.TechnologyUsed,
                    i.Stipend,
                    i.PostedDate,
                    CompanyName = i.Company != null ? i.Company.CompanyName : "Unknown"
                })
                .ToListAsync();
            return Ok(internships);
        }

        [HttpDelete("internships/{id}")]
        public async Task<IActionResult> DeleteInternship(int id)
        {
            var internship = await _context.Internships.FindAsync(id);
            if (internship == null) return NotFound();
            
            // Delete related applications first
            var apps = _context.Applications.Where(a => a.InternshipId == id);
            _context.Applications.RemoveRange(apps);

            _context.Internships.Remove(internship);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Internship and related applications deleted successfully" });
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContactMessages()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return Ok(messages);
        }

        [HttpDelete("contacts/{id}")]
        public async Task<IActionResult> DeleteContactMessage(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();
            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Message deleted successfully" });
        }

        [HttpPut("contacts/{id}/review")]
        public async Task<IActionResult> MarkAsReviewed(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();

            message.IsReviewed = !message.IsReviewed;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Status updated", IsReviewed = message.IsReviewed });
        }
    }
}
