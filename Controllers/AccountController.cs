using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.Data;
using RecruitmentSystem.Models;
using RecruitmentSystem.ViewModels;
using Microsoft.AspNetCore.Http;

namespace RecruitmentSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly RecruitmentContext _context;

        public AccountController(RecruitmentContext context)
        {
            _context = context;
        }

        // Display Register page
        public IActionResult Register()
        {
            return View();
        }

        // Save Applicant
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_context.Applicants.Any(a => a.Email == model.Email))
            {
                ModelState.AddModelError("", "Email already exists.");
                return View(model);
            }

            Applicant applicant = new Applicant
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password
            };

            _context.Applicants.Add(applicant);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var applicant = _context.Applicants
                .FirstOrDefault(a =>
                    a.Email == model.Email &&
                    a.Password == model.Password);

            if (applicant == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("ApplicantId", applicant.ApplicantId);
            return RedirectToAction("Index", "Apply");
        }
    }
}
