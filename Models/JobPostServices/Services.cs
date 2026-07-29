using static SkyTech.Models.JobPostServices.By_TRENCH.Enums.StaffAnalysisEnums;

namespace SkyTech.Models.JobPostServices
{
    public class JobPostDraftRequest
    {
        public string JobTitle { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string EmploymentType { get; set; }
        public double? SalaryMin { get; set; }
        public double? SalaryMax { get; set; }
    }

    //By TRENCH UC1
    public class JobVacancyDraftRequest
    {
        public string JobTitle { get; set; }
        public string Department { get; set; }
        public int HeadcountNeeded { get; set; }
        public VacancyType VacancyType { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public int? ContractDurationDays { get; set; }
    }

    public class JobPostDraftResult
    {
        public string Responsibilities { get; set; }
        public string RequiredCerts { get; set; }
        public string RequiredSkillsAndXP { get; set; }
    }

    public interface IAIJobPostDraftService
    {
        Task<JobPostDraftResult> GenerateDraftAsync(JobPostDraftRequest request);
        Task<JobPostDraftResult> GenerateDraftFromRoleAsync(JobRoleKnowledge role);

        // NEW UC1 method
        Task<JobPostDraftResult> GenerateDraftForVacancyAsync(JobVacancyDraftRequest request);

    }


}
