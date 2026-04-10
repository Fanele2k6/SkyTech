using CM_Project.Models;
using CM_Project.Models.Documents;
using CM_Project.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyTech.Models;
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


        //public IActionResult ReceivedOrders()
        //{
        //    int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

        //    // Normal orders belonging to this branch
        //    var orders = db.Orders
        //        .Include(o => o.OrderItems)
        //            .ThenInclude(i => i.Products)
        //        .Include(o => o.Driver)
        //        .Where(o => o.OwnerId == currentId)
        //        .OrderByDescending(o => o.OrderedOn)
        //        .ToList();

        //    // Transfers OUT — items from this branch going to another pickup branch
        //    var transfersOut = db.InterBranchTransfers
        //        .Include(t => t.OrderItem)
        //            .ThenInclude(oi => oi.Order)
        //                .ThenInclude(o => o.Customers)
        //        .Include(t => t.OrderItem)
        //            .ThenInclude(oi => oi.Products)
        //        .Include(t => t.TargetBranch)
        //        .Include(t => t.Driver)
        //        .Where(t => t.SourceBranchId == currentId
        //                 && t.TransferStatus != ItemTransferStatus.DeliveredToPickupBranch)
        //        .OrderBy(t => t.ScheduledTransferDate)
        //        .ToList();

        //    // Available drivers for transfer assignment
        //    ViewBag.AvailableDrivers = db.DeliveryDrivers
        //        .Where(d => d.IsAvailable && d.ApplicationStatus == Status.Approved)
        //        .ToList();

        //    ViewBag.TransfersOut = transfersOut;

        //    return View(orders);
        //}

        //Separating orders
        // DELIVERY ORDERS - Shows orders delivered to this branch AND items that need to be transported OUT
        public IActionResult DeliveryOrders()
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            // 1. Home Delivery orders belonging to this branch
            var deliveryOrders = db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Products)
                .Include(o => o.Driver)
                .Include(o => o.Customers)
                .Where(o => o.OwnerId == currentId
                            && o.OrderType == DeliveryType.HomeDelivery)
                .OrderByDescending(o => o.OrderedOn)
                .ToList();

            // 2. Items from this branch that need to be transported to another branch (for pickup orders)
            var transfersOut = db.InterBranchTransfers
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                        .ThenInclude(o => o.Customers)
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Products)
                .Include(t => t.TargetBranch)
                .Include(t => t.Driver)
                .Where(t => t.SourceBranchId == currentId
                            && t.TransferStatus != ItemTransferStatus.DeliveredToPickupBranch)
                .OrderBy(t => t.ScheduledTransferDate)
                .ToList();

            ViewBag.TransfersOut = transfersOut;
            ViewBag.AvailableDrivers = db.DeliveryDrivers
                .Where(d => d.IsAvailable && d.ApplicationStatus == Status.Approved)
                .ToList();

            return View(deliveryOrders);
        }

        // PICKUP ORDERS - Shows orders where customer will collect from this branch
        public IActionResult PickupOrders()
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            // Get all pickup orders where this branch is the collection point
            var pickupOrders = db.PickupOrders
                .Include(p => p.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Products)
                .Include(p => p.Order)
                    .ThenInclude(o => o.Customers)
                .Where(p => p.PickupBranchId == currentId)
                .OrderByDescending(p => p.PickupOrderId)
                .ToList();

            // Also get items that have been delivered to this branch from other branches
            var incomingTransfers = db.InterBranchTransfers
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                        .ThenInclude(o => o.Customers)
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Products)
                .Include(t => t.SourceBranch)
                .Where(t => t.TargetBranchId == currentId)
                .OrderBy(t => t.DeliveredToTargetOn)
                .ToList();

            ViewBag.IncomingTransfers = incomingTransfers;

            return View(pickupOrders);
        }

        // Optional: Keep old ReceivedOrders if needed for backward compatibility, or remove it

        public IActionResult AssignDriver(int id)
        {
            var order = db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("DeliveryOrders");
            }

            var availableDrivers = db.DeliveryDrivers
                .Where(d => d.IsAvailable && d.ApplicationStatus == Status.Approved)
                .ToList();

            ViewBag.DeliveryDrivers = availableDrivers;
            ViewBag.IsTransfer = false;

            return View("AssignDriver", order);
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
            return RedirectToAction("DeliveryOrders");
        }

        public IActionResult AssignTransferDriver(int transferId)
        {
            var transfer = db.InterBranchTransfers
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                        .ThenInclude(o => o.Customers)
                .Include(t => t.SourceBranch)
                .Include(t => t.TargetBranch)
                .FirstOrDefault(t => t.TransferId == transferId);

            if (transfer == null)
            {
                TempData["Error"] = "Transfer not found.";
                return RedirectToAction("DeliveryOrders");
            }

            var availableDrivers = db.DeliveryDrivers
                .Where(d => d.IsAvailable && d.ApplicationStatus == Status.Approved)
                .ToList();

            ViewBag.DeliveryDrivers = availableDrivers;
            ViewBag.IsTransfer = true;
            ViewBag.TransferId = transferId;
            ViewBag.TransferData = transfer;

            // Create empty order for the view
            var emptyOrder = new Order();

            return View("AssignDriver", emptyOrder);
        }

        //Assigning the driver for order transfer 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignTransferDriver(int transferId, int driverId)
        {
            var transfer = db.InterBranchTransfers.Find(transferId);
            var driver = db.DeliveryDrivers.Find(driverId);

            if (transfer == null || driver == null)
            {
                TempData["Error"] = "Transfer or driver not found.";
                return RedirectToAction("DeliveryOrders");
            }

            transfer.DriverId = driverId;
            transfer.TransferStatus = ItemTransferStatus.AssignedToDriver;

            db.SaveChanges();

            TempData["Success"] = $"Driver {driver.DriverName} assigned to transfer item {transfer.OrderItem?.ProdName}.";

            return RedirectToAction("DeliveryOrders");
        }

        //Pickup branch orders
        public IActionResult PickupBranchOrders()
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            // All pickup orders where THIS branch is the collection point
            var pickupOrders = db.PickupOrders
                .Include(p => p.Order)
                    .ThenInclude(o => o.Customers)
                .Include(p => p.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Products)
                .Include(p => p.Order)
                    .ThenInclude(o => o.ShopOwner)
                .Where(p => p.PickupBranchId == currentId)
                .OrderByDescending(p => p.Order.OrderedOn)
                .ToList();

            // All inbound transfers coming TO this branch
            var inboundTransfers = db.InterBranchTransfers
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Products)
                .Include(t => t.SourceBranch)
                .Include(t => t.Driver)
                .Where(t => t.TargetBranchId == currentId)
                .ToList();

            ViewBag.InboundTransfers = inboundTransfers;

            return View(pickupOrders);
        }

        [HttpPost]
        public IActionResult ConfirmTransferArrival(int transferId, string transferCode)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var transfer = db.InterBranchTransfers
                .Include(t => t.OrderItem)
                .FirstOrDefault(t => t.TransferId == transferId
                                  && t.TargetBranchId == currentId);

            if (transfer == null)
            {
                TempData["Error"] = "Transfer not found.";
                return RedirectToAction("PickupBranchOrders");
            }

            if (transfer.TransferCode != transferCode.Trim().ToUpper())
            {
                TempData["Error"] = "Incorrect transfer code. Please try again.";
                return RedirectToAction("PickupBranchOrders");
            }

            transfer.IsDelivered = true;
            transfer.TransferStatus = ItemTransferStatus.DeliveredToPickupBranch;
            transfer.DeliveredToTargetOn = DateTime.Now;

            // Check if ALL transfers for this pickup order are now delivered
            // so we can mark the pickup order as ReadyForPickup
            var orderItemOrderId = transfer.OrderItem.OrderId;
            var allTransfersForOrder = db.InterBranchTransfers
                .Include(t => t.OrderItem)
                .Where(t => t.OrderItem.OrderId == orderItemOrderId
                         && t.TargetBranchId == currentId)
                .ToList();

            bool allDelivered = allTransfersForOrder.All(t => t.IsDelivered);
            if (allDelivered)
            {
                var pickupOrder = db.PickupOrders
                    .FirstOrDefault(p => p.OrderId == orderItemOrderId
                                      && p.PickupBranchId == currentId);
                if (pickupOrder != null)
                {
                    pickupOrder.Status = PickupStatus.ReadyForPickup;
                    db.SaveChanges();
                }
            }

            db.SaveChanges();
            TempData["Success"] = "Item arrival confirmed successfully.";
            return RedirectToAction("PickupBranchOrders");
        }

        [HttpPost]
        public IActionResult MarkOrderCollected(int pickupOrderId)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var pickupOrder = db.PickupOrders
                .Include(p => p.Order)
                .FirstOrDefault(p => p.PickupOrderId == pickupOrderId
                                  && p.PickupBranchId == currentId);

            if (pickupOrder == null)
            {
                TempData["Error"] = "Pickup order not found.";
                return RedirectToAction("PickupBranchOrders");
            }

            pickupOrder.Status = PickupStatus.Collected;
            pickupOrder.CollectedOn = DateTime.Now;
            pickupOrder.Order.Status = OrderStatus.Delivered;
            pickupOrder.Order.DeliveredOn = DateTime.Now;

            db.SaveChanges();
            TempData["Success"] = "Order marked as collected.";
            return RedirectToAction("PickupBranchOrders");
        }



    }
}
