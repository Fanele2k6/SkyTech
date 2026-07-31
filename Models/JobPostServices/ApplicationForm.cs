using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SkyTech.Models.JobPostServices
{
    public enum JobApplicationStat
    {
        Pending,Rejected, Online_interview, Physical_Interview
    }
    public class ApplicationForm
    {
        [Key]
        public int ApplicationId { get; set;  }

        [ForeignKey(nameof(Post))]
        public int PostId { get; set; }
        [ValidateNever]
        public virtual JobPost Post { get; set; }

        [ForeignKey(nameof(Customers))]
        public int CustomerId { get; set; }
        [ValidateNever]
        public virtual Customer Customers { get; set; }

        public string FirstName { get; set; }
        //public string SecondName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

        public string IDNumber {  get; set; }

        public string Email {  get; set; }
        public string Location { get; set; }
        public string Experience {  get; set; }
        public string Skills { get; set; }
        public string Certificates { get; set; }
        public string Links { get; set;  }

        public bool HasInternertAccess { get; set; } 
        public DateTime AppliedAt {  get; set; } =DateTime.Now;

        public JobApplicationStat JobStat { get; set; }
        [ValidateNever]
        //============THESE PROPS ARE INGERITED FROM THE JOBPOST TABLE=========
        public string JobTitle { get; set; }
        [ValidateNever]

        public SelectDepartment Department { get; set; }
        [ValidateNever]

        public SelectEmploymentType EmploymentType { get; set; }
        [ValidateNever]

        public double? SalaryMin { get; set; }
        [ValidateNever]

        public double? SalaryMax { get; set; }
    }
}
