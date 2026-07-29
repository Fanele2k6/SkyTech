using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SkyTech.Models
{
    public enum SelectLocation { Cape_Town, Durban, Johannesburg }
    public enum SelectDepartment { Sales_floor, Cash_Desk, Technician, Management }

    public enum SelectEmploymentType { FullTime, Part_Time, }
    public enum CurrentPostStatus { Draft, Published, Deleted }

   
    public class JobPost
    {
        
        [Key]
        public int PostId { get; set; }
        [ForeignKey(nameof(Owner))]
        public int OwnerId {  get; set; }

        [ValidateNever]
        public virtual ShopOwner Owner { get; set; }

        [Required(ErrorMessage = "Job title is required")]
        public string JobTitle { get; set; }

        public SelectDepartment Department { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public SelectLocation Location { get; set; }

        [Required(ErrorMessage = "Employment type is required")]
        [Display(Name = "Employment Type")]
        public SelectEmploymentType EmploymentType { get; set; }

        [Range(0, 999999, ErrorMessage = "Please enter a valid salary amount")]
        public double? SalaryMin { get; set; }

        [Range(0, 999999, ErrorMessage = "Please enter a valid salary amount")]
        public double? SalaryMax { get; set; }

        [Required(ErrorMessage = "Responsibilities are required")]
        [StringLength(3000, ErrorMessage = "Responsibilities cannot exceed 3000 characters")]
        [DataType(DataType.MultilineText)]
        public string Responsibilities { get; set; }

        public string RequiredCerts { get; set; }

        public string RequiredSkillsAndXP { get; set; }

       
        [Required(ErrorMessage = "Closing date is required")]
        [DataType(DataType.Date)]
        public DateTime? ClosingDate { get; set; }

        public CurrentPostStatus PostStatus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}