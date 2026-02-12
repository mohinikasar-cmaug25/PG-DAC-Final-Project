using InnovateConnect.API.Data;
using InnovateConnect.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace InnovateConnect.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<Student>> GetProfile()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                             ?? User.FindFirst("sub")
                             ?? User.FindFirst("id");
            
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var student = await _context.Students
                .Where(s => s.UserId == userId)
                .Select(s => new {
                    s.Id,
                    s.UserId,
                    s.FullName,
                    s.University,
                    s.GitHubLink,
                    s.LeetCodeLink,
                    s.Bio,
                    s.Location,
                    s.Skills,
                    ResumeFileName = s.Resume != null ? s.Resume.ResumeFileName : null
                })
                .FirstOrDefaultAsync();

            if (student == null) return NotFound("Student profile not found.");

            return Ok(student);
        }

        [HttpPut("profile")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateProfile(DTOs.StudentUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                             ?? User.FindFirst("sub")
                             ?? User.FindFirst("id");
            
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) return NotFound("Student profile not found.");

            student.FullName = dto.FullName;
            student.University = dto.University;
            student.GitHubLink = dto.GitHubLink;
            student.LeetCodeLink = dto.LeetCodeLink;
            student.Bio = dto.Bio;
            student.Location = dto.Location;
            student.Skills = dto.Skills;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Profile updated successfully", Student = student });
        }

        [HttpPost("upload-resume")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UploadResume(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".pdf" && extension != ".docx")
                return BadRequest("Only PDF and DOCX files are allowed.");

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                             ?? User.FindFirst("sub")
                             ?? User.FindFirst("id");
            
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) return NotFound("Student profile not found.");

            var resume = await _context.StudentResumes.FirstOrDefaultAsync(r => r.StudentId == student.Id);
            if (resume == null)
            {
                resume = new StudentResume { StudentId = student.Id };
                _context.StudentResumes.Add(resume);
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                resume.ResumeData = memoryStream.ToArray();
                resume.ResumeFileName = file.FileName;
                resume.ResumeContentType = file.ContentType;
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Resume uploaded successfully", FileName = file.FileName });
        }

        [HttpGet("{id}/resume")]
        [AllowAnonymous] // Changed from Authorize to AllowAnonymous to enable direct browser viewing (links don't send JWT)
        public async Task<IActionResult> DownloadResume(int id)
        {
            var resume = await _context.StudentResumes.FirstOrDefaultAsync(r => r.StudentId == id);

            if (resume == null || resume.ResumeData == null)
            {
                return NotFound("Resume not found.");
            }

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{resume.ResumeFileName ?? "resume"}\"";
            return File(resume.ResumeData, resume.ResumeContentType ?? "application/octet-stream");
        }
    }
}
