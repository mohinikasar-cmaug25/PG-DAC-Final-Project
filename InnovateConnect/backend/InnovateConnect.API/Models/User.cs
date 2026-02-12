using System.ComponentModel.DataAnnotations;

namespace InnovateConnect.API.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // "Admin", "Student", "Company"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
