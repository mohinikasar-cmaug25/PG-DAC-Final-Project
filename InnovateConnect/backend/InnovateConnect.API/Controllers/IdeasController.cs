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
    public class IdeasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IdeasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetIdeas()
        {
            return await _context.Ideas
                .Include(i => i.Student)
                .ThenInclude(s => s.User)
                .Select(i => new {
                    i.Id,
                    i.Title,
                    i.Description,
                    TechnologyStack = i.TechnologyUsed,
                    i.PostedDate,
                    Student = new {
                        i.Student.Id,
                        i.Student.FullName,
                        i.Student.University,
                        i.Student.GitHubLink,
                        i.Student.LeetCodeLink,
                        i.Student.Bio,
                        i.Student.Skills,
                        ResumeFileName = i.Student.Resume != null ? i.Student.Resume.ResumeFileName : null,
                        User = new {
                            i.Student.User.Email
                        }
                    }
                })
                .ToListAsync();
        }

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<Idea>>> GetMyIdeas()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"GetMyIdeas: userId from token = {userIdStr}");
            
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            
            var userId = int.Parse(userIdStr);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) {
                Console.WriteLine($"GetMyIdeas: Student profile not found for userId {userId}");
                return BadRequest("Student profile not found.");
            }

            Console.WriteLine($"GetMyIdeas: Fetching ideas for studentId {student.Id}");
            var ideas = await _context.Ideas
                .Where(i => i.StudentId == student.Id)
                .ToListAsync();
            
            Console.WriteLine($"GetMyIdeas: Found {ideas.Count} ideas");
            return ideas;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<Idea>> PostIdea(Idea idea)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            
            if (student == null) return BadRequest("Student profile not found.");

            idea.StudentId = student.Id;
            idea.Student = null; // Prevent cycle or validation error
            idea.PostedDate = DateTime.UtcNow;

            Console.WriteLine($"PostIdea: Saving idea for studentId {student.Id}");
            _context.Ideas.Add(idea);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIdeas", new { id = idea.Id }, idea);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> PutIdea(int id, Idea idea)
        {
            if (id != idea.Id) return BadRequest();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return BadRequest("Student profile not found.");

            var existingIdea = await _context.Ideas.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            if (existingIdea == null) return NotFound();
            if (existingIdea.StudentId != student.Id) return Forbid("You do not own this idea.");

            idea.StudentId = student.Id;
            idea.Student = null;
            idea.PostedDate = existingIdea.PostedDate;

            _context.Entry(idea).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IdeaExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteIdea(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return BadRequest("Student profile not found.");

            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null) return NotFound();

            if (idea.StudentId != student.Id) return Forbid("You do not own this idea.");

            _context.Ideas.Remove(idea);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IdeaExists(int id)
        {
            return _context.Ideas.Any(e => e.Id == id);
        }
    }
}
