using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.ViewModels
{
    public class CreateAssessmentViewModel
    {
        [Required]
        public int JobId { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        [Range(1, 100, ErrorMessage = "Enter a pass mark between 1 and 100.")]
        public int PassMark { get; set; }

        [Required]
        [Range(1, 180)]
        public int TimeLimit { get; set; }

        public List<SelectListItem> Jobs { get; set; }
            = new();
    }
}
