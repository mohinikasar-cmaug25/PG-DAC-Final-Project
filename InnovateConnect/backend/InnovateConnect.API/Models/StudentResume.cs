using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InnovateConnect.API.Models
{
    public class StudentResume
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        public byte[]? ResumeData { get; set; }
        public string? ResumeFileName { get; set; }
        public string? ResumeContentType { get; set; }
    }
}
