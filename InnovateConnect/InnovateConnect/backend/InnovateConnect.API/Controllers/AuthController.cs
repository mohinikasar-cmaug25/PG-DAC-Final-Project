using InnovateConnect.API.Data;
using InnovateConnect.API.DTOs;
using InnovateConnect.API.Models;
using InnovateConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace InnovateConnect.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return BadRequest("Email already exists.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create User
                var user = new User
                {
                    Email = email,
                    Role = dto.Role,
                    PasswordHash = HashPassword(dto.Password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create Role Specific Profile
                if (dto.Role == "Student")
                {
                    var student = new Student
                    {
                        UserId = user.Id,
                        FullName = dto.FullName?.Trim() ?? "Unknown",
                        University = dto.University?.Trim(),
                        GitHubLink = dto.GitHubLink?.Trim(),
                        LeetCodeLink = dto.LeetCodeLink?.Trim(),
                        Location = dto.Location?.Trim()
                    };
                    _context.Students.Add(student);
                }
                else if (dto.Role == "Company")
                {
                    var company = new Company
                    {
                        UserId = user.Id,
                        CompanyName = dto.CompanyName?.Trim() ?? "Unknown",
                        Location = dto.Location?.Trim() ?? "Unknown",
                        Website = dto.Website?.Trim()
                    };
                    _context.Companies.Add(company);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send Registration Email (Outside transaction, try-catch already exists)
                try
                {
                    string name = dto.Role == "Student" ? (dto.FullName ?? "Student") : (dto.CompanyName ?? "Company");
                    string subject = "Welcome to Innovate Connect!";
                    string body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px; text-align: center; color: white;'>
                                <h1>Innovate Connect</h1>
                            </div>
                            <div style='padding: 30px; line-height: 1.6; color: #333;'>
                                <h2>Welcome, {name}!</h2>
                                <p>We're thrilled to have you join our community as a <strong>{dto.Role}</strong>.</p>
                                <p>Innovate Connect is designed to bridge the gap between education and industry, helping {(dto.Role == "Student" ? "students find incredible internships and showcase their ideas" : "companies discover top talent and fresh innovative ideas")}.</p>
                                <div style='margin: 30px 0; text-align: center;'>
                                    <a href='https://innovateconnect.com/login' style='background-color: #667eea; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Login to Your Dashboard</a>
                                </div>
                                <p>If you have any questions, feel free to reply to this email.</p>
                                <p>Best Regards,<br/><strong>The Innovate Connect Team</strong></p>
                            </div>
                            <div style='background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #777;'>
                                &copy; 2026 Innovate Connect. All rights reserved.
                            </div>
                        </div>";
                    
                    await _emailService.SendEmailAsync(email, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending registration email: {ex.Message}");
                }

                return Ok(new { Message = "Registration successful" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                if (ex.InnerException?.Message.Contains("Duplicate entry") == true)
                {
                    return BadRequest("Email already exists.");
                }
                return StatusCode(500, "An error occurred while saving to the database.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.PasswordHash != HashPassword(dto.Password))
                return Unauthorized("Invalid email or password.");

            var token = GenerateJwtToken(user);
            
            // Get Name
            string name = user.Email;
            if(user.Role == "Student") {
               var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
               if(student != null) name = student.FullName;
            } else if (user.Role == "Company") {
               var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == user.Id);
               if(company != null) name = company.CompanyName;
            }

            return Ok(new { Token = token, User = new { user.Id, user.Email, user.Role, Name = name } });
        }

        [HttpGet("verify")]
        [Authorize]
        public async Task<IActionResult> Verify()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            // Get Name
            string name = user.Email;
            if (user.Role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (student != null) name = student.FullName;
            }
            else if (user.Role == "Company")
            {
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (company != null) name = company.CompanyName;
            }

            return Ok(new { user.Id, user.Email, user.Role, Name = name });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Sub MUST be the ID for default mapping to work with int.Parse
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            // Simple SHA256 for demo purposes. Use BCrypt or Argon2 in production!
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
