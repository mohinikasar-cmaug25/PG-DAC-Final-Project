using InnovateConnect.API.Data;
using InnovateConnect.API.Models;
using InnovateConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InnovateConnect.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public ApplicationsController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Apply for an Internship (Student)
        [HttpPost("{internshipId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Apply(int internshipId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return BadRequest("Student profile not found.");

            var exists = await _context.Applications.AnyAsync(a => a.StudentId == student.Id && a.InternshipId == internshipId);
            if (exists) return BadRequest("Already applied.");

            var application = new Application
            {
                InternshipId = internshipId,
                StudentId = student.Id,
                Status = "Applied",
                AppliedDate = DateTime.UtcNow
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Applied successfully." });
        }

        // Get My Applications (Student)
        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return BadRequest("Student profile not found.");

            var apps = await _context.Applications
                .Include(a => a.Internship)
                .ThenInclude(i => i.Company)
                .Where(a => a.StudentId == student.Id)
                .ToListAsync();

            return Ok(apps);
        }

        // Get Applications for Company (Company)
        [HttpGet("company")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> GetCompanyApplications()
        {
             var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
             var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);
             if (company == null) return BadRequest("Company profile not found.");

             var apps = await _context.Applications
                 .Include(a => a.Student)
                 .Include(a => a.Internship)
                 .Where(a => a.Internship!.CompanyId == company.Id)
                 .ToListAsync();
             
             return Ok(apps);
        }

        // Update Status (Company) - Accept/Reject
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
             var application = await _context.Applications
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Internship)
                    .ThenInclude(i => i.Company)
                .FirstOrDefaultAsync(a => a.Id == id);
             
             if (application == null) return NotFound();

             // Verify ownership
             var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
             var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);
             if (company == null || application.Internship!.CompanyId != company.Id) return Unauthorized();

             application.Status = status; // "Accepted" or "Rejected"
             await _context.SaveChangesAsync();

             if (status == "Accepted")
             {
                 try
                 {
                     string studentEmail = application.Student?.User?.Email;
                     if (!string.IsNullOrEmpty(studentEmail))
                     {
                           string subject = $"Congratulations! Application Accepted: {application.Internship.Title}";
                          string body = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                                <div style='background: linear-gradient(135deg, #48bb78 0%, #38a169 100%); padding: 20px; text-align: center; color: white;'>
                                    <h1>Application Accepted!</h1>
                                </div>
                                <div style='padding: 30px; line-height: 1.6; color: #333;'>
                                    <h2>Congratulations, {application.Student.FullName}!</h2>
                                    <p>Your application for the internship <strong>{application.Internship.Title}</strong> has been <strong>ACCEPTED</strong> by <strong>{application.Internship.Company.CompanyName}</strong>.</p>
                                    
                                    <div style='background-color: #f0fff4; border-left: 4px solid #48bb78; padding: 20px; margin: 20px 0;'>
                                        <h3 style='margin-top: 0; color: #2f855a;'>Internship Details:</h3>
                                        <table style='width: 100%; border-collapse: collapse;'>
                                            <tr>
                                                <td style='padding: 8px 0; color: #555; width: 40%;'><strong>Company:</strong></td>
                                                <td style='padding: 8px 0;'>{application.Internship.Company.CompanyName}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; color: #555;'><strong>Role:</strong></td>
                                                <td style='padding: 8px 0;'>{application.Internship.Title}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; color: #555;'><strong>Stipend:</strong></td>
                                                <td style='padding: 8px 0;'>${application.Internship.Stipend}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; color: #555;'><strong>Technology:</strong></td>
                                                <td style='padding: 8px 0;'>{application.Internship.TechnologyUsed}</td>
                                            </tr>
                                        </table>
                                    </div>
                                    
                                    <p><strong>Next Steps:</strong></p>
                                    <p>The company will reach out to you directly via your registered email to discuss the onboarding process and other details.</p>
                                    
                                    <div style='margin: 30px 0; text-align: center;'>
                                        <a href='https://innovateconnect.com/student-dashboard' style='background-color: #38a169; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>View on Dashboard</a>
                                    </div>
                                    
                                    <p>Best Regards,<br/><strong>The Innovate Connect Team</strong></p>
                                </div>
                                <div style='background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #777;'>
                                    &copy; 2026 Innovate Connect. All rights reserved.
                                </div>
                            </div>";

                         await _emailService.SendEmailAsync(studentEmail, subject, body);
                     }
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"Error sending acceptance email: {ex.Message}");
                 }
             }

             return Ok(new { Message = "Status updated.", NewStatus = status });
        }
    }
}
