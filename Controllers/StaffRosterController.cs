using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyTech.Models;
using SkyTech.Models.SkytechWorkers;

namespace SkyTech.Controllers
{
    public class StaffRosterController : Controller
    {
        private readonly ApplicationDbContext db;

        public StaffRosterController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult StaffDash()
        {
            return View();
        }

        public async Task<IActionResult> MyRoster()
        {
            int? staffId = HttpContext.Session.GetInt32("UserId");
            if (staffId == null) return RedirectToAction("Login", "Authentication");

            var loggedInStaff = await db.Staffs
                .Include(s => s.Positions)
                .FirstOrDefaultAsync(s => s.StaffId == staffId.Value);

            if (loggedInStaff == null) return RedirectToAction("Login", "Authentication");

            // Find the most recent PUBLISHED roster for this staff member's branch
            var roster = await db.Rosters
                .Include(r => r.Branch)
                .Include(r => r.RosterShifts).ThenInclude(s => s.Staff)
                .Include(r => r.RosterShifts).ThenInclude(s => s.Position)
                .Where(r => r.BranchId == loggedInStaff.BranchId &&
                            r.Status == RosterStatus.Published)
                .OrderByDescending(r => r.WeekStartDate)
                .FirstOrDefaultAsync();

            if (roster == null)
            {
                ViewBag.NoRoster = true;
                ViewBag.CurrentStaffId = loggedInStaff.StaffId;
                return View();
            }

            // Only shifts for THIS staff member's position — but showing everyone in that position
            var positionShifts = roster.RosterShifts
                .Where(s => s.PositionId == loggedInStaff.PositionId)
                .ToList();

            ViewBag.PositionName = loggedInStaff.Positions?.PositionName;
            ViewBag.WeekStartDate = roster.WeekStartDate;
            ViewBag.BranchName = roster.Branch.BranchName;
            ViewBag.CurrentStaffId = loggedInStaff.StaffId;

            return View(positionShifts);
        }
    }
}