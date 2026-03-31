using CM_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CM_Project.Controllers
{
    [AuthorizeUser]
    public class SystemAdminController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly EmailServices _services;

        // ✅ ONE constructor only
        public SystemAdminController(ApplicationDbContext context, EmailServices services)
        {
            db = context;
            _services = services;
        }

        public IActionResult SystemAdminDash() => View();

        // ── SHOP OWNER MANAGEMENT ─────────────────────────────────────

        public IActionResult ViewApplications()
        {
            var applications = db.shopowners
                .Include(s => s.OwnerIdentityBooks)
                .Include(s => s.BussRegCertificates)
                .ToList();
            return View(applications);
        }

        public IActionResult DownloadBusRegCert(int id)
        {
            var doc = db.BusRegCertificates.Find(id);
            return doc == null ? NotFound() : File(doc.FileData, doc.FileType);
        }

        public IActionResult DownloadOwnerID(int id)
        {
            var doc = db.OwnerIdentityBooks.Find(id);
            return doc == null ? NotFound() : File(doc.FileData, doc.FileType);
        }

        public IActionResult ApproveAppl(int id)
        {
            var app = db.shopowners.Find(id);
            if (app != null)
            {
                app.ApplicationStatus = ApplicationStat.Approved;
                db.SaveChanges();
                _services.SOwnerApplApproved(app.Email, app.Names);
            }
            return RedirectToAction("ViewApplications");
        }

        public IActionResult RejectAppl(int id)
        {
            var app = db.shopowners.Find(id);
            if (app != null)
            {
                app.ApplicationStatus = ApplicationStat.Rejected;
                db.SaveChanges();
                _services.SOwnerApplRejected(app.Email, app.Names);
            }
            return RedirectToAction("ViewApplications");
        }

        // ── DRIVER MANAGEMENT ────────────────────────────────────────

        public IActionResult ViewDriverApplication()
        {
            var applications = db.DeliveryDrivers
                .Include(s => s.DriversLicences)
                .Include(s => s.DriverIdentityBooks)
                .ToList();
            return View(applications);
        }

        public IActionResult DownloadDriverIdentityBooks(int id)
        {
            var doc = db.DriverIdentityBooks.Find(id);
            return doc == null ? NotFound() : File(doc.FileData, doc.FileType);
        }

        public IActionResult DownloadLicence(int id)
        {
            var doc = db.DriversLicences.Find(id);
            return doc == null ? NotFound() : File(doc.FileData, doc.FileType);
        }

        public IActionResult ApproveDriverAppl(int id)
        {
            var app = db.DeliveryDrivers.Find(id);
            if (app != null)
            {
                app.ApplicationStatus = Status.Approved;
                db.Entry(app).Property(d => d.ApplicationStatus).IsModified = true;
                db.SaveChanges();
                _services.DriveAppliApproved(app.Email, app.DriverName);
            }
            return RedirectToAction("ViewDriverApplication");
        }

        public IActionResult RejectDriverAppl(int id)
        {
            var app = db.DeliveryDrivers.Find(id);
            if (app != null)
            {
                app.ApplicationStatus = Status.Rejected;
                db.SaveChanges();
                _services.DriveAppliReject(app.Email, app.DriverName);
            }
            return RedirectToAction("ViewDriverApplication");
        }
    }
}
