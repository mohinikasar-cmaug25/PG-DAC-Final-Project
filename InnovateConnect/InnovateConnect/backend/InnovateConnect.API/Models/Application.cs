using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InnovateConnect.API.Models
{
    public class Application
    {
        public int Id { get; set; }

        public int InternshipId { get; set; }
        [ForeignKey("InternshipId")]
        public Internship? Internship { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Required]
        public string Status { get; set; } = "Applied"; // Applied, Accepted, Rejected

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    }
}
