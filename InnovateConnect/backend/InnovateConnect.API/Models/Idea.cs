using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InnovateConnect.API.Models
{
    public class Idea
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string TechnologyUsed { get; set; } = string.Empty;

        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    }
}
