using Microsoft.AspNetCore.Mvc;
using SkyTech.Models;
using SkyTech.Models.JobPostServices;

namespace SkyTech.Controllers
{
    public class JobRoleKnowledgeController : Controller
    {
        private readonly ApplicationDbContext db;
        public JobRoleKnowledgeController(ApplicationDbContext db) { this.db = db; }

        public IActionResult Index()
        {
            var roles = db.JobRoleKnowledges.OrderBy(r => r.JobTitle).ToList();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobRoleKnowledge model)
        {
            if (!ModelState.IsValid)
                return View("JobRoleKnowledge/Create", model);

            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            db.Add(model);
            await db.SaveChangesAsync();
            TempData["successful"] = "Role information saved";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = await db.JobRoleKnowledges.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(JobRoleKnowledge model)
        {
            if (!ModelState.IsValid)
                return View("JobRoleKnowledge/Edit", model);

            var role = await db.JobRoleKnowledges.FindAsync(model.RoleId);
            if (role == null) return NotFound();

            role.JobTitle = model.JobTitle;
            role.CompanyResponsibilities = model.CompanyResponsibilities;
            role.RequiredSkills = model.RequiredSkills;
            role.RequiredExperience = model.RequiredExperience;
            role.RequiredQualifications = model.RequiredQualifications;
            role.OtherCompanyInformation = model.OtherCompanyInformation;
            role.IsActive = model.IsActive;
            role.UpdatedAt = DateTime.Now;

            await db.SaveChangesAsync();
            TempData["successful"] = "Role information updated";
            return RedirectToAction("Index");
        }
    }
}