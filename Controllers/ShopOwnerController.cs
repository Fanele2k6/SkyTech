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

            //Now filters directly on Order.OwnerId instead of OrderItems
            var pendingOrdersCount = db.Orders
                .Where(i => i.OwnerId == currentId && i.Status == OrderStatus.Pending)
                .Count();

            var completed = db.Orders
                .Where(i => i.OwnerId == currentId && i.Status == OrderStatus.Delivered)
                .Count();

            var totalStoreSales = db.Orders
                .Where(oi => oi.Status == OrderStatus.Delivered && oi.OwnerId == currentId)
                .Sum(oi => (double?)oi.TotalAmount) ?? 0;

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
          
            //var sessionId = 
            int currentId = HttpContext.Session.GetInt32("UserId").Value;

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
                ProdType = model.ProdType,
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
            product.ProdType = model.ProdType;
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

     
        public IActionResult ReceivedOrders()
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var orders = db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Products)
                .Include(o => o.Driver)
                .Where(o => o.OwnerId == currentId)
                .OrderByDescending(o => o.OrderedOn)
                .ToList();

            return View(orders);
        }

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
            if (order == null)
                return Json(new { success = false, reason = "Order not found" });

            var driver = db.DeliveryDrivers.Find(driverId);
            if (driver == null)
                return Json(new { success = false, reason = "Driver not found" });

            // Count how many orders this driver is already assigned for
            // on the same scheduled delivery date as this order
            int ordersOnSameDay = db.Orders.Count(o =>
                o.DriverId == driverId &&
                o.ScheduledDeliveryDate.Date == order.ScheduledDeliveryDate.Date &&
                o.Status != OrderStatus.Delivered);

            // Block assignment if driver already has 5 orders for that day
            if (ordersOnSameDay >= 5)
                return Json(new { success = false, reason = "This driver already has 5 orders assigned for that day. Please choose another driver." });

            // Assign driver
            order.DriverId = driverId;
            order.Status = OrderStatus.Assigned;

            // Check total orders assigned for that day after this assignment
            int totalAfterAssignment = ordersOnSameDay + 1;

            // Mark driver unavailable if they now have 5 orders for that day
            if (totalAfterAssignment >= 5)
                driver.IsAvailable = false;

            db.SaveChanges();
            return RedirectToAction("ReceivedOrders");
        }

        //===================================Return Actions======================================
        //public IActionResult ReturnedItems()
        //{
        //    int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

        //    var pendingReturns = db.OrderItems
        //        .Include(i => i.Order)
        //            .ThenInclude(o => o.Customers)
        //        .Include(i => i.Products)
        //            .ThenInclude(p => p.ProductPictures)
        //        .Where(i => i.ReturnPStatus == ReturnProduct.Return_Pending
        //                 && i.Order.OwnerId == currentId)   // ← only this shop's orders
        //        .OrderByDescending(i => i.Order.DeliveredOn)
        //        .ToList();

        //    return View(pendingReturns);
        //}

        //// Approve the return
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ApproveReturn(int orderItemId)
        //{
        //    int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

        //    var item = db.OrderItems
        //        .Include(i => i.Order)
        //        .FirstOrDefault(i => i.OrderItemId == orderItemId
        //                          && i.Order.OwnerId == currentId);

        //    if (item == null) return NotFound();

        //    // Find an available, approved driver
        //    var availableDriver = db.DeliveryDrivers
        //        .FirstOrDefault(d => d.IsAvailable == true
        //                          && d.ApplicationStatus == Status.Approved);

        //    if (availableDriver == null)
        //    {
        //        TempData["Error"] = "No drivers are currently available. Please try again later.";
        //        return RedirectToAction("ReturnedItems");
        //    }

        //    // Assign the driver to the order item
        //    item.ReturnPStatus = ReturnProduct.Collection_Assigned;
        //    item.DriverId = availableDriver.DriverId;
        //    item.IsAssingedForCollection = true;
        //    item.CollectionDate = DateTime.Now.AddDays(1);
        //    item.NewDeliveryDate = DateTime.Now.AddDays(4);

        //    // Mark the driver as unavailable
        //    availableDriver.IsAvailable = false;

        //    db.SaveChanges();

        //    TempData["Message"] = $"Return for '{item.ProdName}' approved and assigned to driver {availableDriver.DriverName} {availableDriver.DriverSurname}.";
        //    return RedirectToAction("ReturnedItems");
        //}

        // Shows ALL active returns (not just pending)
        public IActionResult ReturnedItems()
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var activeReturns = db.OrderItems
                .Include(i => i.Order)
                    .ThenInclude(o => o.Customers)
                .Include(i => i.Products)
                    .ThenInclude(p => p.ProductPictures)
                .Include(i => i.DeliveryDriver)
                .Where(i => i.Order.OwnerId == currentId
                         && i.ReturnPStatus != ReturnProduct.No_Request
                         && i.ReturnPStatus != ReturnProduct.Completed)
                .OrderByDescending(i => i.Order.DeliveredOn)
                .ToList();

            return View(activeReturns);
        }

        // Step 1 — shop owner approves & assigns collection driver
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveReturn(int orderItemId)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var item = db.OrderItems
                .Include(i => i.Order)
                .FirstOrDefault(i => i.OrderItemId == orderItemId
                                  && i.Order.OwnerId == currentId);

            if (item == null) return NotFound();

            var availableDriver = db.DeliveryDrivers
                .FirstOrDefault(d => d.IsAvailable == true
                                  && d.ApplicationStatus == Status.Approved);

            if (availableDriver == null)
            {
                TempData["Error"] = "No drivers are currently available. Please try again later.";
                return RedirectToAction("ReturnedItems");
            }

            item.ReturnPStatus = ReturnProduct.Collection_Assigned;
            item.DriverId = availableDriver.DriverId;
            item.IsAssingedForCollection = true;
            item.CollectionDate = DateTime.Now.AddDays(1);
            item.NewDeliveryDate = DateTime.Now.AddDays(4);

            availableDriver.IsAvailable = false;
            db.SaveChanges();

            TempData["Message"] = $"Return for '{item.ProdName}' approved. Driver {availableDriver.DriverName} {availableDriver.DriverSurname} assigned for collection.";
            return RedirectToAction("ReturnedItems");
        }

        // Step 2 — mark item as collected (driver picked it up, now being inspected)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkItemCollected(int orderItemId)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var item = db.OrderItems
                .Include(i => i.Order)
                .Include(i => i.DeliveryDriver)
                .FirstOrDefault(i => i.OrderItemId == orderItemId
                                  && i.Order.OwnerId == currentId
                                  && i.ReturnPStatus == ReturnProduct.Collection_Assigned);

            if (item == null) return NotFound();

            // Free up the collection driver
            if (item.DeliveryDriver != null)
                item.DeliveryDriver.IsAvailable = true;

            item.ReturnPStatus = ReturnProduct.Item_Collected;
            item.DriverId = null;   // clear driver — new one assigned in next step

            db.SaveChanges();

            TempData["Message"] = $"'{item.ProdName}' marked as collected and is now being inspected/replaced.";
            return RedirectToAction("ReturnedItems");
        }

        // Step 3 — assign a driver to deliver the replacement/exchange item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignExchangeDriver(int orderItemId)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var item = db.OrderItems
                .Include(i => i.Order)
                .FirstOrDefault(i => i.OrderItemId == orderItemId
                                  && i.Order.OwnerId == currentId
                                  && i.ReturnPStatus == ReturnProduct.Item_Collected);

            if (item == null) return NotFound();

            var availableDriver = db.DeliveryDrivers
                .FirstOrDefault(d => d.IsAvailable == true
                                  && d.ApplicationStatus == Status.Approved);

            if (availableDriver == null)
            {
                TempData["Error"] = "No drivers available for re-delivery. Please try again later.";
                return RedirectToAction("ReturnedItems");
            }

            item.ReturnPStatus = ReturnProduct.Exchange_Dispatched;
            item.DriverId = availableDriver.DriverId;
            item.IsAssingedForSecDelivery = true;
            item.NewDeliveryDate = DateTime.Now.AddDays(1);

            availableDriver.IsAvailable = false;
            db.SaveChanges();

            TempData["Message"] = $"Replacement for '{item.ProdName}' dispatched. Driver {availableDriver.DriverName} {availableDriver.DriverSurname} assigned for delivery.";
            return RedirectToAction("ReturnedItems");
        }

        public IActionResult AssignCollection() => View();


    }
}
