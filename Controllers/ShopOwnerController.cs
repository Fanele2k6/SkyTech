using CM_Project.Models;
using CM_Project.Models.Documents;
using CM_Project.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace CM_Project.Controllers
{
    [AuthorizeUser]
    public class ShopOwnerController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly EmailServices _services = null;  // ← add this
        private readonly IWebHostEnvironment _webHostEnvironment;


        public ShopOwnerController(ApplicationDbContext context, EmailServices services, IWebHostEnvironment webHostEnvironment)
        {
            db = context;
            _services = services;
            _webHostEnvironment = webHostEnvironment;
        }
        //public ShopOwnerController(ApplicationDbContext context) { db = context; }

        public IActionResult ShopOwnerDash()
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;

            var appStat = db.shopowners
                .Where(a => a.OwnerId == currentId)
                .Select(a => a.ApplicationStatus)
                .FirstOrDefault();

            var drivers = db.DeliveryDrivers
                .Count(d => d.ApplicationStatus == Status.Approved && d.IsAvailable);

            var pendingOrdersCount = db.OrderItems
                .Where(i => i.Products.OwnerId == currentId && i.Order.Status == OrderStatus.Pending)
                .Select(i => i.OrderId).Distinct().Count();

            var completed = db.OrderItems
                .Where(i => i.Products.OwnerId == currentId && i.Order.Status == OrderStatus.Delivered)
                .Select(i => i.OrderId).Distinct().Count();

            var totalStoreSales = db.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Delivered && oi.Products.OwnerId == currentId)
                .Sum(oi => (double?)oi.PriceAtTimeOfPurchase * oi.Quantity) ?? 0;

            // EF Core: EF.Functions.DateDiffWeek replaces SqlFunctions.DatePart
            var weeklySales = db.OrderItems
                 .Include(o => o.Order)        // Added the two includes Order and Products
                 .Include(o => o.Products)
                .Where(o => o.Products.OwnerId == currentId && o.Order.Status == OrderStatus.Delivered)
                .AsEnumerable() // pulls to memory first
                .GroupBy(o => System.Globalization.ISOWeek.GetWeekOfYear(o.Order.OrderedOn))
                .Select(g => new { Week = g.Key, Total = g.Sum(x => x.PriceAtTimeOfPurchase * x.Quantity) })
                .OrderBy(x => x.Week)
                .ToList();

            ViewBag.WeeklySalesLabels = weeklySales.Select(x => "Week " + x.Week).ToList();
            ViewBag.WeeklySalesData = weeklySales.Select(x => x.Total).ToList();
            ViewBag.ApplicationStatus = appStat;
            ViewBag.AvailableDrivers = drivers;
            ViewBag.PendingOrders = pendingOrdersCount;
            ViewBag.CompletedOrders = completed;
            ViewBag.TotalSales = totalStoreSales;

            return View(new ShopOwner());
        }

        [HttpGet]
        public IActionResult AddProductPage() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProductPage(AddProductViewModel model)
        {
            if (!ModelState.IsValid) return View("AddProductPage");
           // int currentId = HttpContext.Session.GetInt32("UserId").Value;

           
            // Check if session has a value at all
            var sessionId = HttpContext.Session.GetInt32("UserId");
            if (sessionId == null)
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("LogIn", "Authentication");
            }

            int currentId = sessionId.Value;

            // Verify this OwnerId actually exists in the shopowners table
            var ownerExists = db.shopowners.Any(o => o.OwnerId == currentId);
            if (!ownerExists)
            {
                TempData["Error"] = $"Owner with ID {currentId} not found. Session may be storing the wrong ID.";
                return View("AddProductPage");
            }

            var addProduct = new AddProduct
            {
                OwnerId = currentId,
                ProdName = model.ProdName,
                ProdCategory = model.ProdCategory,
                ProdQuantity = model.ProdQuantity,
                ProdDescription = model.ProdDescription,
                ProdPrice = model.ProdPrice,
                ProdSize = model.ProdSize,
                ProductPictures = new List<ProductPicture>(),
                Status = model.ProdQuantity == 0 ? ProdStatus.Out_Of_Stock
                                 : model.ProdQuantity <= 10 ? ProdStatus.Low_stock
                                                            : ProdStatus.In_Stock
            };

            if (model.ProductPicture != null && model.ProductPicture.Length > 0)
            {
                // using var ms = new MemoryStream();
                // model.ProductPicture.CopyTo(ms);

                 var pictureFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "ProductPicture");
                 Directory.CreateDirectory(pictureFolder);
                
                 var fileName = $"{currentId}_{Guid.NewGuid()}{Path.GetExtension(model.ProductPicture.FileName)}";
                 var filePath = Path.Combine(pictureFolder, fileName);
                
                 using (var stream = new FileStream(filePath, FileMode.Create))
                 {
                     model.ProductPicture.CopyTo(stream);
                 }

                addProduct.ProductPictures.Add(new ProductPicture
                {
                    OwnerId = currentId,
                   // FileData = ms.ToArray(),
                    FilePath = $"/Uploads/ProductPicture/{fileName}",
                    FileName = Path.GetFileName(model.ProductPicture.FileName),
                    FileType = model.ProductPicture.ContentType,
                    SavedOn = DateTime.Now
                });
            }

            db.AddProducts.Add(addProduct);
            db.SaveChanges();

            TempData["Message"] = "Product added successfully!";
            return RedirectToAction("AddProductPage", "ShopOwner");
        }

        [HttpGet]
        public IActionResult EditProductPage()
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;
            var product = db.AddProducts.FirstOrDefault(o => o.OwnerId == currentId);

            if (product == null) return NotFound();

            return View(new AddProductViewModel
            {
                ProdName = product.ProdName,
                ProdCategory = product.ProdCategory,
                ProdQuantity = product.ProdQuantity,
                ProdDescription = product.ProdDescription,
                ProdPrice = product.ProdPrice,
                ProdSize = product.ProdSize
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProductPage(AddProductViewModel model, IFormFile ProductPicture)
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;
            var product = db.AddProducts
                .Include(p => p.ProductPictures)
                .FirstOrDefault(o => o.OwnerId == currentId);

            if (product == null) return NotFound();

            product.ProdName = model.ProdName;
            product.ProdCategory = model.ProdCategory;
            product.ProdQuantity = model.ProdQuantity;
            product.ProdDescription = model.ProdDescription;
            product.ProdPrice = model.ProdPrice;
            product.ProdSize = model.ProdSize;


            if (ProductPicture != null && ProductPicture.Length > 0)
            {
                // Save new file to disk
                var pictureFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "ProductPicture");
                Directory.CreateDirectory(pictureFolder);
            
                var fileName = $"{currentId}_{Guid.NewGuid()}{Path.GetExtension(ProductPicture.FileName)}";
                var filePath = Path.Combine(pictureFolder, fileName);
            
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProductPicture.CopyTo(stream);
                }
            
                var existingPic = product.ProductPictures.FirstOrDefault();
                if (existingPic != null)
                {
                    // Delete old file from disk before replacing
                    var oldFullPath = Path.Combine(_webHostEnvironment.WebRootPath, existingPic.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFullPath))
                        System.IO.File.Delete(oldFullPath);
            
                    // Update existing record with new path
                    existingPic.FilePath = $"/Uploads/ProductPicture/{fileName}";
                    existingPic.FileName = Path.GetFileName(ProductPicture.FileName);
                    existingPic.FileType = ProductPicture.ContentType;
                    existingPic.SavedOn = DateTime.Now;
                }
                else
                {
                    // No picture existed before, add a new one
                    product.ProductPictures.Add(new ProductPicture
                    {
                        OwnerId = currentId,
                        FilePath = $"/Uploads/ProductPicture/{fileName}",
                        FileName = Path.GetFileName(ProductPicture.FileName),
                        FileType = ProductPicture.ContentType,
                        SavedOn = DateTime.Now
                    });
                }

            // if (ProductPicture != null && ProductPicture.Length > 0)
            // {
            //     using var ms = new MemoryStream();
            //     ProductPicture.CopyTo(ms);
            //     byte[] fileBytes = ms.ToArray();
            //     var existingPic = product.ProductPictures.FirstOrDefault();

            //     if (existingPic != null)
            //     {
            //         existingPic.FileData = fileBytes;
            //         existingPic.FileName = Path.GetFileName(ProductPicture.FileName);
            //         existingPic.FileType = ProductPicture.ContentType;
            //         existingPic.SavedOn = DateTime.Now;
            //     }
            //     else
            //     {
            //         product.ProductPictures.Add(new ProductPicture
            //         {
            //             OwnerId = currentId,
            //             FileData = fileBytes,
            //             FileName = Path.GetFileName(ProductPicture.FileName),
            //             FileType = ProductPicture.ContentType,
            //             SavedOn = DateTime.Now
            //         });
            //     }
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Product updated successfully!";
            return RedirectToAction("TrackStockLevelPage");
        }

        public IActionResult TrackStockLevelPage()
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;
            var allProds = db.AddProducts
                .Include(p => p.ProductPictures)
                .Where(p => p.OwnerId == currentId)
                .ToList();
            return View(allProds);
        }

        [HttpGet]
        public IActionResult StaffReg() => View();

        [HttpPost]
        public IActionResult StaffReg(StaffViewModel model)
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;

            if (!ModelState.IsValid) return View("StaffReg");

            if (db.Customers.Any(d => d.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View("StaffReg");
            }

            db.Staffs.Add(new Staff
            {
                OwnerId = currentId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password,
                UserType = "Staff",
                RegisteredOn = DateTime.Now
            });
            db.SaveChanges();

            //new Services().StaffRegConfirmation(model.Email, model.FirstName);
            _services.CustomerRegConfirmation(model.Email, model.FirstName);
            TempData["Message"] = "Registration successful";
            return RedirectToAction("ShopOwnerDash");
        }

        public IActionResult StaffMembers()
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;
            return View(db.Staffs.Where(p => p.OwnerId == currentId).ToList());
        }

        public IActionResult ManageDeliveriesPage() => View();

        public IActionResult OwnerProfile()
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;
            return View(db.shopowners.Where(p => p.OwnerId == currentId).ToList());
        }

        //public IActionResult ReceivedOrders()
        //{
        //    var orders = db.Orders
        //        .Include(o => o.OrderItems)
        //        .Include(o => o.Driver)
        //        .OrderByDescending(o => o.OrderedOn)
        //        .ToList();
        //    return View(orders);
        //}

        public IActionResult ReceivedOrders()
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var orders = db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Products)
                .Include(o => o.Driver)
                .Where(o => o.OrderItems.Any(i => i.Products.OwnerId == currentId))
                .OrderByDescending(o => o.OrderedOn)
                .ToList();

            return View(orders);
        }

        //public IActionResult AssignDriver(int id)
        //{
        //    var order = db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderId == id);

        //    ViewBag.DeliveryDrivers = db.DeliveryDrivers
        //        .Where(d => d.IsAvailable && d.ApplicationStatus == Status.Approved)
        //        .ToList();

        //    return View(order);
        //}

        public IActionResult AssignDriver(int id)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            // Make sure this order actually belongs to this shop owner
            var order = db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Products)
                .FirstOrDefault(o => o.OrderId == id
                    && o.OrderItems.Any(i => i.Products.OwnerId == currentId));

            if (order == null) return Forbid(); // stops other owners accessing it via URL

            ViewBag.DeliveryDrivers = db.DeliveryDrivers
                .Where(d => d.IsAvailable)
                .ToList();

            return View(order);
        }

        [HttpPost]
        public IActionResult AssignDriver(int orderId, int driverId)
        {
            var order = db.Orders.Find(orderId);
            var driver = db.DeliveryDrivers.Find(driverId);

            if (order != null && driver != null)
            {
                order.DriverId = driverId;
                order.Status = OrderStatus.Assigned;
                driver.IsAvailable = false;
                db.SaveChanges();
            }

            return RedirectToAction("ReceivedOrders");
        }
    }
}
