using Microsoft.AspNetCore.Mvc;
using SkyTech.Models;
using SkyTech.Models.JobPostServices.By_TRENCH;
using static SkyTech.Models.JobPostServices.By_TRENCH.Enums.StaffAnalysisEnums;
using Microsoft.EntityFrameworkCore;


namespace SkyTech.Controllers.By_TRENCH
{
    public class StaffRequestingLeave : Controller
    {



        private readonly ApplicationDbContext _db;

        public StaffRequestingLeave(ApplicationDbContext db)
        {
            _db = db;
        }

        // Staff view: Request a leave
        [HttpGet]
        public async Task<IActionResult> RequestLeave()
        {
            var leaveTypes = await _db.LeaveTypes.ToListAsync();
            ViewBag.LeaveTypes = leaveTypes;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLeave(LeaveRequest request)
        {
            int staffId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (staffId == 0)
            {
                TempData["error"] = "Please log in first.";
                return RedirectToAction("Login", "Auth");
            }

            // Validate dates
            if (request.EndDate < request.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
            }

            if (request.EndDate < DateTime.UtcNow)
            {
                ModelState.AddModelError("EndDate", "Cannot request leave in the past.");
            }

            if (!ModelState.IsValid)
            {
                var leaveTypes = await _db.LeaveTypes.ToListAsync();
                ViewBag.LeaveTypes = leaveTypes;
                return View(request);  // Only one return, pass the request back to view
            }

            var leaveRequest = new LeaveRequest
            {
                StaffId = staffId,
                LeaveTypeId = request.LeaveTypeId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Reason = request.Reason,
                Status = LeaveStatus.Pending,
                AppliedOn = DateTime.UtcNow
            };

            _db.LeaveRequests.Add(leaveRequest);
            await _db.SaveChangesAsync();

            TempData["success"] = "Leave request submitted successfully. Awaiting approval.";
            return RedirectToAction("MyLeaveRequests");

        }

        // Staff view: See their own leave requests
        [HttpGet]
        public async Task<IActionResult> MyLeaveRequests()
        {
            int staffId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var leaves = await _db.LeaveRequests
                .Where(l => l.StaffId == staffId)
                .Include(l => l.LeaveType)
                .Include(l => l.ApprovedByStaff)
                .OrderByDescending(l => l.AppliedOn)
                .ToListAsync();

            return View(leaves);
        }

        // HR/ShopOwner view: Pending leave requests to review
        [HttpGet]
        public async Task<IActionResult> PendingLeaveRequests()
        {
            var leaves = await _db.LeaveRequests
                .Where(l => l.Status == LeaveStatus.Pending)
                .Include(l => l.Staff)
                .Include(l => l.LeaveType)
                .OrderBy(l => l.StartDate)
                .ToListAsync();

            return View(leaves);
        }

        // HR/ShopOwner: Approve leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            var leave = await _db.LeaveRequests.Include(l => l.Staff).FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null)
                return NotFound();

            int approverId = HttpContext.Session.GetInt32("UserId") ?? 0;

            leave.Status = LeaveStatus.Approved;
            leave.ApprovedByStaffId = approverId;
            leave.ApprovedOn = DateTime.UtcNow;

            // Mark staff as OnLeave
            leave.Staff.EmploymentStatus = EmploymentStatus.OnLeave;

            await _db.SaveChangesAsync();

            TempData["success"] = $"Leave request approved for {leave.Staff.FirstName} {leave.Staff.LastName}";
            return RedirectToAction("PendingLeaveRequests");
        }

        // HR/ShopOwner: Reject leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLeave(int id, string rejectionReason)
        {
            var leave = await _db.LeaveRequests.Include(l => l.Staff).FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null)
                return NotFound();

            leave.Status = LeaveStatus.Rejected;
            leave.ApprovedByStaffId = HttpContext.Session.GetInt32("UserId");
            leave.ApprovedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["success"] = $"Leave request rejected for {leave.Staff.FirstName} {leave.Staff.LastName}";
            return RedirectToAction("PendingLeaveRequests");
        }
    }
}



