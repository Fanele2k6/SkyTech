using CM_Project.Models;
using CM_Project.Models.Documents;
using CM_Project.Models.forgot;
using CM_Project.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace CM_Project.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly EmailServices _services;// = null;  // ← add this
        
        //Added this namespace, angikazi ngokwanele ukuthi owani
        //But ngoba engu web host environment i think uyena owenza kukhandeke i path ku wwwroot, kungasaveki ku db
        private readonly IWebHostEnvironment _webHostEnvironment; 


        // ✅ Fix it to include email
        public AuthenticationController(ApplicationDbContext context, EmailServices services, IWebHostEnvironment webHostEnvironment)
        {
            db = context;
            _services = services;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Home() => View();

        [HttpGet]
        public IActionResult LogIn() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogIn(LogInViewModel models)
        {
            if (!ModelState.IsValid) return View(models);

            // ShopOwner
            var shopOwner = db.shopowners
                .FirstOrDefault(d => d.Email == models.Email && d.Password == models.Password);
            if (shopOwner != null)
            {
                HttpContext.Session.SetInt32("UserId", shopOwner.OwnerId);
                HttpContext.Session.SetString("UserType", "ShopOwner");
                return RedirectToAction("ShopOwnerDash", "ShopOwner");
            }

            // Customer
            var cus = db.Customers
                .FirstOrDefault(d => d.Email == models.Email && d.Password == models.Password);
            if (cus != null)
            {
                HttpContext.Session.SetInt32("UserId", cus.CustomerId);
                HttpContext.Session.SetString("UserType", "Customer");
                return RedirectToAction("ShoppingPage", "Customer");
            }

            // SystemAdmin
            var systemAdmin = db.SystemAdmins
                .FirstOrDefault(d => d.Email == models.Email && d.Password == models.Password);
            if (systemAdmin != null)
            {
                HttpContext.Session.SetInt32("UserId", systemAdmin.AdminId);
                HttpContext.Session.SetString("UserType", "SystemAdmin");
                return RedirectToAction("SystemAdminDash", "SystemAdmin");
            }

            // Staff
            var staff = db.Staffs
                .FirstOrDefault(n => n.Email == models.Email && n.Password == models.Password);
            if (staff != null)
            {
                HttpContext.Session.SetInt32("UserId", staff.StaffId);
                HttpContext.Session.SetString("UserType", "Staff");
                return RedirectToAction("ShopOwnerDash", "ShopOwner");
            }

            // Driver
            var driver = db.DeliveryDrivers
                .FirstOrDefault(n => n.Email == models.Email && n.Password == models.Password);
            if (driver != null)
            {
                HttpContext.Session.SetInt32("UserId", driver.DriverId);
                HttpContext.Session.SetString("UserType", "Driver");
                return RedirectToAction("DriverDash", "DeliveryDriver");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(models);
        }

        [HttpGet]
        public IActionResult CustomerReg() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerReg(CustomerViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (db.Customers.Any(d => d.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(model);
            }

            var customer = new Customer
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                UserType = "Customer",
                RegisteredOn = DateTime.Now
            };

            db.Customers.Add(customer);
            db.SaveChanges();

            //new Services().CustomerRegConfirmation(model.Email, model.FirstName);
            _services.CustomerRegConfirmation(model.Email, model.FirstName);
            TempData["Message"] = "Registration successful";
            return RedirectToAction("LogIn");
        }

        [HttpGet]
        public IActionResult ShopOwnerReg() => View();

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ShopOwnerReg(ShopOwnerViewModel model, IFormFile BusRegCertificate, IFormFile OwnerIdentityBook)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    if (db.shopowners.Any(d => d.Email == model.Email))
        //    {
        //        ModelState.AddModelError("Email", "Email already exists");
        //        return View(model);
        //    }

        //    var shopOwner = new ShopOwner
        //    {
        //        IdBookNumber = model.IdBookNumber,
        //        Names = model.Names,
        //        Surname = model.Surname,
        //        Email = model.Email,
        //        ResidentialAddress = model.ResidentialAddress,
        //        AccountNumber = model.AccountNumber,
        //        BankType = model.BankType,
        //        ShopName = model.ShopName,
        //        ShopAddress = model.ShopAddress,
        //        Shoptype = model.Shoptype,
        //        Password = model.Password,
        //        ApplicationStatus = ApplicationStat.Pending,
        //        UserType = "ShopOwner",
        //        RegisteredOn = DateTime.Now
        //    };

        //    db.shopowners.Add(shopOwner);
        //    db.SaveChanges();

        //    int currentId = shopOwner.OwnerId;

        //    if (OwnerIdentityBook != null && OwnerIdentityBook.Length > 0)
        //    {
        //        using var ms = new MemoryStream();
        //        OwnerIdentityBook.CopyTo(ms);
        //        db.OwnerIdentityBooks.Add(new OwnerIdentityBook
        //        {
        //            OwnerId = currentId,
        //            FileName = OwnerIdentityBook.FileName,
        //            FileType = OwnerIdentityBook.ContentType,
        //            FileData = ms.ToArray(),
        //            SavedOn = DateTime.Now
        //        });
        //    }

        //    if (BusRegCertificate != null && BusRegCertificate.Length > 0)
        //    {
        //        using var ms = new MemoryStream();
        //        BusRegCertificate.CopyTo(ms);
        //        db.BusRegCertificates.Add(new BusRegCertificate
        //        {
        //            OwnerId = currentId,
        //            FileName = BusRegCertificate.FileName,
        //            FileType = BusRegCertificate.ContentType,
        //            FileData = ms.ToArray(),
        //            SavedOn = DateTime.Now
        //        });
        //    }

        //    db.SaveChanges();

        //    _services.CustomerRegConfirmation(model.Email, model.Names);
        //    TempData["Message"] = "Registration successful";
        //    return RedirectToAction("LogIn");
        //}

        //this is the controller

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ShopOwnerReg(ShopOwnerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (db.shopowners.Any(d => d.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(model);
            }

            var shopOwner = new ShopOwner
            {
                IdBookNumber = model.IdBookNumber,
                Names = model.Names,
                Surname = model.Surname,
                Email = model.Email,
                ResidentialAddress = model.ResidentialAddress,
                AccountNumber = model.AccountNumber,
                BankType = model.BankType,
                ShopName = model.ShopName,
                ShopAddress = model.ShopAddress,
                Shoptype = model.Shoptype,
                Password = model.Password,
                ApplicationStatus = ApplicationStat.Pending,
                UserType = "ShopOwner",
                RegisteredOn = DateTime.Now
            };

            db.shopowners.Add(shopOwner);
            db.SaveChanges();

            //int currentId = shopOwner.OwnerId;
            if (model.OwnerIdentityBook != null && model.OwnerIdentityBook.Length > 0)
            {
                // using var ms = new MemoryStream();
                // model.OwnerIdentityBook.CopyTo(ms);
                
                
                var idFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "OwnerIdBook"); //Path
                Directory.CreateDirectory(idFolder); //this one creates a new folder if it does not exist

                // Build a unique filename using the  ID + a random GUID + the original file extension
                //this prevents ovewriting the file if users upload the file with the same name
                var idFileName = $"{shopOwner.OwnerId}_{Guid.NewGuid()}{Path.GetExtension(model.OwnerIdentityBook.FileName)}";
                var idFilePath = Path.Combine(idFolder, idFileName); // Combine the folder path and the filename to get the full path where the file will be saved

                using (var stream = new FileStream(idFilePath, FileMode.Create))
                {
                    model.OwnerIdentityBook.CopyTo(stream);
                }
                
                db.OwnerIdentityBooks.Add(new OwnerIdentityBook
                {
                    OwnerId = shopOwner.OwnerId,
                    FileName = model.OwnerIdentityBook.FileName,
                    FileType = model.OwnerIdentityBook.ContentType,
                   // FileData = ms.ToArray(), Remove this one
                     FilePath = $"/Uploads/OwnerIdBook/{idFileName}", //Store relative path
                    SavedOn = DateTime.Now
                });
                //db.SaveChanges();
            }

            if (model.BusRegCertificate != null && model.BusRegCertificate.Length > 0)
            {
                // using var ms = new MemoryStream();
                // model.BusRegCertificate.CopyTo(ms);

                 var busRegFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "RegCertificate");//path
                 Directory.CreateDirectory(busRegFolder); //creating new folder if does not exist
                
                 var busRegFileName = $"{shopOwner.OwnerId}_{Guid.NewGuid()}{Path.GetExtension(model.BusRegCertificate.FileName)}";
                 var busRegFilePath = Path.Combine(busRegFolder, busRegFileName);
                
                 using (var stream = new FileStream(busRegFilePath, FileMode.Create))
                 {
                     model.BusRegCertificate.CopyTo(stream);
                 }

                db.BusRegCertificates.Add(new BusRegCertificate
                {
                    OwnerId = shopOwner.OwnerId,
                    FileName = model.BusRegCertificate.FileName,
                    FileType = model.BusRegCertificate.ContentType,
                   // FileData = ms.ToArray(),
                   FilePath = $"/Uploads/RegCertificate/{busRegFileName}", //store relative path
                    SavedOn = DateTime.Now
                });
                //db.SaveChanges();
            }

            db.SaveChanges();
            _services.CustomerRegConfirmation(model.Email, model.Names);
            TempData["Message"] = "Registration successful";
            return RedirectToAction("LogIn");
        }


        [HttpGet]
        public IActionResult DeliveryDriverReg() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeliveryDriverReg(DeliveryDriverViewModel model,
            IFormFile DriverIdentityBooks, IFormFile DriversLicences)
        {
            if (!ModelState.IsValid) return View(model);

            if (db.DeliveryDrivers.Any(d => d.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(model);
            }

            var driver = new DeliveryDriver
            {
                Email = model.Email,
                DriverName = model.DriverName,
                DriverSurname = model.DriverSurname,
                PhoneNumber = model.PhoneNumber,
                ResidentialAddress = model.ResidentialAddress,
                LicenceNumber = model.LicenceNumber,
                VehicleType = model.VehicleType,
                VehicleNumber = model.VehicleNumber,
                AccountNumber = model.AccountNumber,
                IsAvailable = true,
                ApplicationStatus = Status.Pending,
                Password = model.Password,
                UserType = "Driver",
                RegisteredOn = DateTime.Now
            };

            db.DeliveryDrivers.Add(driver);
            db.SaveChanges();

            //int currentId = driver.DriverId;

            if (model.DriversLicences != null && model.DriversLicences.Length > 0)
            {
                // using var ms = new MemoryStream();
                // DriversLicences.CopyTo(ms);

                 var licenceFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "DrivingLicence"); //Path
                 Directory.CreateDirectory(licenceFolder); // creates folder if it doesn't exist
                
                 var licenceFileName = $"{driver.DriverId}_{Guid.NewGuid()}{Path.GetExtension(model.DriversLicence.FileName)}";
                 var licenceFilePath = Path.Combine(licenceFolder, licenceFileName);
                
                 using (var stream = new FileStream(licenceFilePath, FileMode.Create))
                 {
                     model.DriversLicence.CopyTo(stream);
                 }

                
                db.DriversLicences.Add(new DriversLicence
                {
                    DriverId = driver.DriverId,
                    FileName = model.DriversLicences.FileName,
                    FileType = model.DriversLicences.ContentType,
                  //  FileData = ms.ToArray(),
                   FilePath = $"/Uploads/DrivingLicence/{licenceFileName}", // store relative path
                    SavedOn = DateTime.Now
                });
            }

            if (model.DriverIdentityBooks != null && model.DriverIdentityBooks.Length > 0)
            {
                // using var ms = new MemoryStream();
                // DriverIdentityBooks.CopyTo(ms);
                
                  var idFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "DriverIdDocument");
                  Directory.CreateDirectory(idFolder);
                
                  var idFileName = $"{driver.DriverId}_{Guid.NewGuid()}{Path.GetExtension(model.DriverIdentityBook.FileName)}";
                  var idFilePath = Path.Combine(idFolder, idFileName);
                
                  using (var stream = new FileStream(idFilePath, FileMode.Create))
                  {
                      model.DriverIdentityBook.CopyTo(stream);
                  }
                
                db.DriverIdentityBooks.Add(new DriverIdentityBook
                {
                    DriverId = driver.DriverId,
                    FileName = model.DriverIdentityBooks.FileName,
                    FileType = model.DriverIdentityBooks.ContentType,
                   // FileData = ms.ToArray(),
                   FilePath = $"/Uploads/DriverIdDocument/{idFileName}",
                    SavedOn = DateTime.Now
                });
            }

            db.SaveChanges();
            //new Services().DriverRegConfirmation(model.Email, model.DriverName);
            _services.CustomerRegConfirmation(model.Email, model.DriverName);
            TempData["Message"] = "Registration successful";
            return RedirectToAction("LogIn", "Authentication");
        }

        // ── FORGOT PASSWORD ──────────────────────────────────────────────

        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            bool userExists = db.Customers.Any(d => d.Email == model.UserEmail)
                           || db.Staffs.Any(n => n.Email == model.UserEmail)
                           || db.shopowners.Any(n => n.Email == model.UserEmail)
                           || db.DeliveryDrivers.Any(n => n.Email == model.UserEmail);

            if (!userExists)
            {
                ModelState.AddModelError("UserEmail", "Email not found.");
                return View(model);
            }

            string otp = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("ResetEmail", model.UserEmail);
            HttpContext.Session.SetString("OtpExpiry", DateTime.Now.AddMinutes(5).ToString("o"));

            //new Services().SendOtpEmail(model.UserEmail, otp);
            _services.CustomerRegConfirmation(model.UserEmail, otp);
            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp() =>
            View(new VerifyOtpViewModel { UserEmail = HttpContext.Session.GetString("ResetEmail") });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOtp(VerifyOtpViewModel model)
        {
            string sessionOtp = HttpContext.Session.GetString("OTP");
            string expiryStr = HttpContext.Session.GetString("OtpExpiry");
            DateTime expiry = expiryStr != null ? DateTime.Parse(expiryStr) : DateTime.MinValue;

            if (DateTime.Now > expiry)
            {
                ModelState.AddModelError("", "OTP expired. Please request again.");
                return View(model);
            }

            if (model.OtpCode == sessionOtp)
                return RedirectToAction("ResetPassword");

            ModelState.AddModelError("OtpCode", "Invalid OTP.");
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword() =>
            View(new ResetPasswordViewModel { UserEmail = HttpContext.Session.GetString("ResetEmail") });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var customer = db.Customers.FirstOrDefault(d => d.Email == model.UserEmail);
            var staff = db.Staffs.FirstOrDefault(n => n.Email == model.UserEmail);
            var driver = db.DeliveryDrivers.FirstOrDefault(n => n.Email == model.UserEmail);
            var shopowner = db.shopowners.FirstOrDefault(n => n.Email == model.UserEmail);

            if (customer != null)
            {
                customer.Password = model.NewPassword;
                customer.ConfirmPassword = model.NewPassword;
            }
            else if (staff != null)
            {
                staff.Password = model.NewPassword;
            }
            else if (driver != null)
            {
                driver.Password = model.NewPassword;
                driver.ConfirmPassword = model.NewPassword;
            }
            else if (shopowner != null)
            {
                shopowner.Password = model.NewPassword;
                shopowner.ConfirmPassword = model.NewPassword;
            }
            else
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            db.SaveChanges();

            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("ResetEmail");
            HttpContext.Session.Remove("OtpExpiry");

            TempData["Message"] = "Password reset successful. Please log in.";
            return RedirectToAction("LogIn");
        }
    }
}
