using System.ComponentModel.DataAnnotations;

namespace InnovateConnect.API.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Student, Company

        // Student Fields
        public string? FullName { get; set; }
        public string? University { get; set; }
        public string? GitHubLink { get; set; }
        public string? LeetCodeLink { get; set; }

        // Company Fields
        public string? CompanyName { get; set; }
        public string? Location { get; set; }
        public string? Website { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class StudentUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public string? GitHubLink { get; set; }
        public string? LeetCodeLink { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? Skills { get; set; }
    }

    public class CompanyUpdateDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Website { get; set; }
    }
}
