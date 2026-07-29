using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyTech.Models;
using SkyTech.Models.JobPostServices.By_TRENCH;

namespace SkyTech.Controllers.By_TRENCH
{
    public class StaffingAnalysisAdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IStaffingAnalysisService _analysisService;

        public StaffingAnalysisAdminController(ApplicationDbContext db, IStaffingAnalysisService analysisService)
        {
            _db = db;
            _analysisService = analysisService;
        }


        // View all schedules
        [HttpGet]
        public async Task<IActionResult> Schedules()
        {
            var schedules = await _db.StaffingAnalysisSchedules
                .Include(s => s.SystemAdmin)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(schedules);
        }

        // Create new schedule
        [HttpGet]
        public IActionResult CreateSchedule()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule(StaffingAnalysisSchedule schedule, string TimeOfDay)
        {
            int adminId = HttpContext.Session.GetInt32("UserId")!.Value;

            if (!string.IsNullOrEmpty(TimeOfDay))
            {
                var parts = TimeOfDay.Split(':');
                schedule.TimeOfDay = new TimeSpan(int.Parse(parts[0]), int.Parse(parts[1]), 0);
            }

            schedule.SystemAdminId = adminId;
            schedule.CreatedAt = DateTime.UtcNow;
            schedule.CronExpression = "";  // Set default
            schedule.Runs = new List<StaffingAnalysisRun>();  // Initialize

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach(var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(schedule);
            }

            _db.StaffingAnalysisSchedules.Add(schedule);
            await _db.SaveChangesAsync();

            TempData["success"] = "Schedule created successfully. It will run at the specified time.";
            return RedirectToAction("Schedules");
        }

        // Manually trigger analysis
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunNow(int scheduleId)
        {
            int adminId = HttpContext.Session.GetInt32("UserId")!.Value;

            try
            {
                var adminIdObj = HttpContext.Session.GetInt32("UserId");

                if (!adminIdObj.HasValue)
                {
                    TempData["error"] = "Admin ID not found in session. Please log in again.";
                    return RedirectToAction("RunHistory", new { scheduleId });
                }


                var run = await _analysisService.RunAnalysisAsync(scheduleId: scheduleId, triggeredByAdminId: adminId);
                TempData["success"] = $"Analysis started. Generated {run.RecommendationsGenerated} recommendations.";
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                TempData["error"] = $"Analysis failed: {ex.Message}";
               
            }

            return RedirectToAction("RunHistory", new { scheduleId });
        }

        // View analysis run history
        [HttpGet]
        public async Task<IActionResult> RunHistory(int scheduleId)
        {
            var runs = await _db.StaffingAnalysisRuns
                .Where(r => r.ScheduleId == scheduleId)
                .OrderByDescending(r => r.StartedAt)
                .ToListAsync();

            ViewBag.ScheduleId = scheduleId;
            return View(runs);
        }

        // View recommendations from a run
        [HttpGet]
        public async Task<IActionResult> Recommendations(int runId)
        {
            var recommendations = await _db.JobVacancyRecommendations
                .Where(r => r.AnalysisRunId == runId)
                .Include(r => r.Department)
                .Include(r => r.Position)
                .Include(r => r.LeaveRequest)
                .OrderBy(r => r.VacancyType)
                .ToListAsync();

            ViewBag.RunId = runId;
            return View(recommendations);
        }

        // Deactivate schedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateSchedule(int id)
        {
            var schedule = await _db.StaffingAnalysisSchedules.FindAsync(id);
            if (schedule == null)
                return NotFound();

            schedule.IsActive = false;
            await _db.SaveChangesAsync();

            TempData["success"] = "Schedule deactivated.";
            return RedirectToAction("Schedules");
        }


       
    }
}
