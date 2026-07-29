using Microsoft.EntityFrameworkCore;
using SkyTech.Models;
using SkyTech.Models.JobPostServices;
using SkyTech.Models.JobPostServices.By_TRENCH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SkyTech.Models.JobPostServices.By_TRENCH.Enums.StaffAnalysisEnums;

public interface IStaffingAnalysisService
{
    Task<StaffingAnalysisRun> RunAnalysisAsync(int? scheduleId = null, int? triggeredByAdminId = null);
}

public class StaffingAnalysisService : IStaffingAnalysisService
{
    private readonly ApplicationDbContext _db;
    private readonly IAIJobPostDraftService _aiService;

    public StaffingAnalysisService(ApplicationDbContext db, IAIJobPostDraftService aiService)
    {
        _db = db;
        _aiService = aiService;
    }

    public async Task<StaffingAnalysisRun> RunAnalysisAsync(int? scheduleId = null, int? triggeredByAdminId = null)
    {

        Console.WriteLine($"[UC1] RunAnalysisAsync called - scheduleId: {scheduleId}, adminId: {triggeredByAdminId}");

        var trigger = triggeredByAdminId.HasValue ? AnalysisTrigger.ManualByAdmin : AnalysisTrigger.Scheduled;

        var run = new StaffingAnalysisRun
        {
            ScheduleId = scheduleId,
            TriggeredByAdminId = triggeredByAdminId,
            Trigger = trigger,
            Status = AnalysisRunStatus.Running,
            StartedAt = DateTime.UtcNow
        };

        _db.StaffingAnalysisRuns.Add(run);
        await _db.SaveChangesAsync();

        Console.WriteLine($"[UC1] Run {run.Id} created successfully");

        try
        {
            var activeDepartments = await _db.Departments
                .Where(d => d.IsActive)
                .ToListAsync();

            var activePositions = await _db.Positions
                .Where(p => p.IsActive)
                .ToListAsync();

            int deptCount = 0;
            int understaffedCount = 0;
            int recommendationsCreated = 0;

            foreach (var dept in activeDepartments)
            {
                foreach (var position in activePositions.Where(p => p.DepartmentId == dept.Id))
                {
                    deptCount++;

                    var allStaffInPosition = await _db.Staffs
                        .Where(s => s.PositionId == position.Id && s.EmploymentStatus != EmploymentStatus.Terminated)
                        .ToListAsync();

                    var activeCount = allStaffInPosition
                        .Count(s => s.EmploymentStatus == EmploymentStatus.Active);

                    var onLeaveCount = allStaffInPosition
                        .Count(s => s.EmploymentStatus == EmploymentStatus.OnLeave);

                    var resignedCount = allStaffInPosition
                        .Count(s => s.EmploymentStatus == EmploymentStatus.Resigned);

                    int vacancyCount = position.MaxHeadcount - activeCount;
                    bool isUnderstaffed = vacancyCount > 0;

                    if (isUnderstaffed)
                        understaffedCount++;

                    var snapshot = new DepartmentStaffingSnapshot
                    {
                        AnalysisRunId = run.Id,
                        DepartmentId = dept.Id,
                        PositionId = position.Id,
                        RequiredHeadcount = position.MaxHeadcount,
                        ActiveEmployeeCount = activeCount,
                        OnLeaveCount = onLeaveCount,
                        ResignedRecentCount = resignedCount,
                        VacancyCount = vacancyCount,
                        IsUnderstaffed = isUnderstaffed
                    };

                    _db.DepartmentStaffingSnapshots.Add(snapshot);
                    await _db.SaveChangesAsync();

                    if (isUnderstaffed)
                    {
                        // PERMANENT VACANCY
                        if (vacancyCount > 0)
                        {
                            var permanentRec = await CreateAndGenerateRecommendationAsync(
                                run, snapshot, dept, position,
                                VacancyType.Permanent,
                                headcountNeeded: vacancyCount,
                                leaveRequestId: null
                            );
                            recommendationsCreated++;
                        }

                        // TEMPORARY LEAVE-COVER
                        var staffOnLeave = allStaffInPosition
                            .Where(s => s.EmploymentStatus == EmploymentStatus.OnLeave)
                            .ToList();

                        foreach (var staffMember in staffOnLeave)
                        {
                            var activeLeaves = await _db.LeaveRequests
                                .Where(l => l.StaffId == staffMember.StaffId
                                    && l.Status == LeaveStatus.Approved
                                    && l.StartDate <= DateTime.UtcNow
                                    && l.EndDate >= DateTime.UtcNow)
                                .ToListAsync();

                            foreach (var leave in activeLeaves)
                            {
                                var tempRec = await CreateAndGenerateRecommendationAsync(
                                    run, snapshot, dept, position,
                                    VacancyType.TemporaryLeaveCover,
                                    headcountNeeded: 1,
                                    leaveRequestId: leave.Id,
                                    contractStartDate: leave.StartDate,
                                    contractEndDate: leave.EndDate
                                );
                                recommendationsCreated++;
                            }
                        }
                    }
                }
            }

            run.Status = AnalysisRunStatus.Completed;
            run.CompletedAt = DateTime.UtcNow;
            run.DepartmentsAnalyzed = deptCount;
            run.UnderstaffedPositionsFound = understaffedCount;
            run.RecommendationsGenerated = recommendationsCreated;

            await _db.SaveChangesAsync();

            if (scheduleId.HasValue)
            {
                var schedule = await _db.StaffingAnalysisSchedules.FindAsync(scheduleId.Value);
                if (schedule != null)
                {
                    schedule.LastRunAt = DateTime.UtcNow;
                    schedule.NextRunAt = CalculateNextRunTime(schedule);
                    await _db.SaveChangesAsync();
                }
            }

            return run;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UC1] Error in analysis: {ex.Message}");
            Console.WriteLine($"[UC1] Stack trace: {ex.StackTrace}");
            throw;

            run.Status = AnalysisRunStatus.Failed;
            run.ErrorMessage = ex.Message;
            run.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            throw;
        }
    }

    private async Task<JobVacancyRecommendation> CreateAndGenerateRecommendationAsync(
        StaffingAnalysisRun run,
        DepartmentStaffingSnapshot snapshot,
        Department department,
        Position position,
        VacancyType vacancyType,
        int headcountNeeded,
        int? leaveRequestId,
        DateTime? contractStartDate = null,
        DateTime? contractEndDate = null)
    {
        var aiRequest = new JobVacancyDraftRequest
        {
            JobTitle = position.Title,
            Department = department.Name,
            HeadcountNeeded = headcountNeeded,
            VacancyType = vacancyType,
            ContractStartDate = contractStartDate,
            ContractEndDate = contractEndDate
        };

        JobPostDraftResult draft;
        try
        {
            draft = await _aiService.GenerateDraftForVacancyAsync(aiRequest);
        }
        catch (Exception ex)
        {
            draft = new JobPostDraftResult
            {
                Responsibilities = "Generated content unavailable; please edit manually.",
                RequiredCerts = "Not specified",
                RequiredSkillsAndXP = "Not specified"
            };
        }

        var rec = new JobVacancyRecommendation
        {
            AnalysisRunId = run.Id,
            SnapshotId = snapshot.Id,
            DepartmentId = department.Id,
            PositionId = position.Id,
            SuggestedJobTitle = position.Title,
            SuggestedResponsibilities = draft.Responsibilities,
            SuggestedRequiredCerts = draft.RequiredCerts,
            SuggestedRequiredSkillsAndXP = draft.RequiredSkillsAndXP,
            RecommendedHeadcount = headcountNeeded,
            VacancyType = vacancyType,
            LeaveRequestId = leaveRequestId,
            ContractStartDate = contractStartDate,
            ContractEndDate = contractEndDate,
            ContractDurationDays = contractEndDate.HasValue && contractStartDate.HasValue
                ? (int)(contractEndDate.Value.Date - contractStartDate.Value.Date).TotalDays
                : null,
            Status = RecommendationStatus.Draft,
            GeneratedAt = DateTime.UtcNow
        };

        _db.JobVacancyRecommendations.Add(rec);
        await _db.SaveChangesAsync();

        return rec;
    }

    private DateTime? CalculateNextRunTime(StaffingAnalysisSchedule schedule)
    {
        if (!schedule.IsActive)
            return null;

        switch (schedule.Frequency)
        {
            case AnalysisFrequency.Daily:
                return DateTime.UtcNow.AddDays(1).Date + schedule.TimeOfDay;

            case AnalysisFrequency.Weekly:
                return DateTime.UtcNow.AddDays(7).Date + schedule.TimeOfDay;

            case AnalysisFrequency.Monthly:
                var next = DateTime.UtcNow.AddMonths(1);
                return new DateTime(next.Year, next.Month, 1) + schedule.TimeOfDay;

            case AnalysisFrequency.Custom:
                return null; // TODO: Parse CronExpression

            default:
                return null;
        }
    }
}