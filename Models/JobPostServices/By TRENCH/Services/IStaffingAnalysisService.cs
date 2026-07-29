namespace SkyTech.Models.JobPostServices.By_TRENCH.Services
{
    public interface IStaffingAnalysisService
    {
        Task<StaffingAnalysisRun> RunAnalysisAsync(int? scheduleId = null, int? triggeredByAdminId = null);
    }
}
