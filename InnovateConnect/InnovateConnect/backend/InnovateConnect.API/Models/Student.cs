using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InnovateConnect.API.Models
{
    public class Student
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? GitHubLink { get; set; } 
        public string? LeetCodeLink { get; set; }
        public string? University { get; set; }

        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? Skills { get; set; } // Comma separated for MVP

        public StudentResume? Resume { get; set; }
    }
}
