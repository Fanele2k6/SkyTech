using System.ComponentModel.DataAnnotations;
namespace RecruitmentSystem.Models
{
    public class Applicant
    {
        [Key]
        public int ApplicantId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]

        public string Password { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Application>? Applications { get; set; }
    }
}
