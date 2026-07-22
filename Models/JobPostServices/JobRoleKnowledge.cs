// Models/JobRoleKnowledge.cs
using System.ComponentModel.DataAnnotations;

namespace SkyTech.Models.JobPostServices
{
    public class JobRoleKnowledge
    {
        [Key]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Job title is required")]
        [StringLength(150)]
        public string JobTitle { get; set; }

        [Required(ErrorMessage = "Responsibilities are required")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Company Responsibilities (one per line)")]
        public string CompanyResponsibilities { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Required Skills (one per line)")]
        public string RequiredSkills { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Required Experience (one per line)")]
        public string RequiredExperience { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Required Qualifications (one per line)")]
        public string RequiredQualifications { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Other Company Information")]
        public string OtherCompanyInformation { get; set; }

        // Lets HR retire a role without deleting history
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}