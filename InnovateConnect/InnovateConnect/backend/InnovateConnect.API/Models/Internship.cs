using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InnovateConnect.API.Models
{
    public class Internship
    {
        public int Id { get; set; }

        public int? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string TechnologyUsed { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Stipend { get; set; }

        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    }
}
