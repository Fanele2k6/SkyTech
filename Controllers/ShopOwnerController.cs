using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkyTech.Models;
using SkyTech.Models.Documents;
using SkyTech.Models.JobPostServices;
using SkyTech.Services;
using SkyTech.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.Xml;
using static SkyTech.Models.JobPostServices.By_TRENCH.Enums.StaffAnalysisEnums;

namespace SkyTech.Controllers
{
    [AuthorizeUser]
    public class ShopOwnerController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly EmailServices _services = null;  // ← add this
        private readonly IWebHostEnvironment _webHostEnvironment;

        //paying customer their money
        private readonly PayStackTransferService _transfer;
        private readonly IAIJobPostDraftService _aiService;

        //public ShopOwnerController()
        //{
        //    //this.db = db;
        //}

        public ShopOwnerController(ApplicationDbContext context, EmailServices services, IWebHostEnvironment webHostEnvironment, PayStackTransferService transfer, IAIJobPostDraftService aiService)
        {
            //this.db = db;

            db = context;
            _transfer = transfer;
            _aiService = aiService;
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
            product.Status = model.ProdQuantity == 0 ? ProdStatus.Out_Of_Stock     // the product status was not updating so i added this.
                                 : model.ProdQuantity <= 10 ? ProdStatus.Low_stock
                                                            : ProdStatus.In_Stock;


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

        //=================================Increament 2 ==========================

        //public IActionResult EvaluateProposal() => View();
        public IActionResult EvaluateProposal()
        {
            var proposals = db.ProposeItems
                .Include(p => p.ReceiptPics)
                .Include(p => p.ProposedProdPics)
                .Where(p => p.ProdStatus == Status.Pending) // ← only show pending
                .ToList();

            return View(proposals);
        }

        // action to open the partial view so that the shop owner can enter the proposed amount 
        [HttpGet]
        public IActionResult AcceptProposal(int id)
        {
            var proposal = db.ProposeItems.Find(id);
            if (proposal == null) return NotFound();

            return PartialView("_AcceptProposalModal", proposal);
        }

        [HttpPost]
        public IActionResult AcceptProposal(int id, double amount)
        {
            var proposal = db.ProposeItems.Find(id);
            if (proposal == null) return NotFound();

            int currentId = HttpContext.Session.GetInt32("UserId").Value;

            // Fetch the logged-in shop owner's address
            var shopOwner = db.shopowners.FirstOrDefault(s => s.OwnerId == currentId);

            proposal.AmountProposed = amount;
            proposal.StoreAddress = shopOwner?.ShopAddress ?? "Address not found";
            proposal.ProdStatus = Status.Approved;

            db.SaveChanges();

            return RedirectToAction("EvaluateProposal");
        }

        // this one declines the proposal
        [HttpPost]
        public IActionResult DeclineProposal(int id)
        {
            var proposal = db.ProposeItems.Find(id);
            if (proposal == null) return NotFound();

            proposal.ProdStatus = Status.Rejected;

            db.SaveChanges();

            return RedirectToAction("EvaluateProposal");
        }

        public IActionResult AcceptedProposals()
        {
            var proposals = db.ProposeItems
                .Include(p => p.ReceiptPics)
                .Include(p => p.ProposedProdPics)
                .Include(p => p.Checkout)
                .Where(p => p.ProdStatus == Status.Approved ||
                            p.ProdStatus == Status.CustomerAccepted ||
                            p.ProdStatus == Status.Item_Collected ||
                            p.ProdStatus == Status.CollectionScheduled ||
                            p.ProdStatus == Status.CustomerAccepted)
                .ToList();

            return View(proposals);
        }
        // GET — shows confirmation page
        [HttpGet]
        public IActionResult ScheduleCollection(int id)
        {
            var proposal = db.ProposeItems
                .Include(p => p.Checkout)
                .FirstOrDefault(p => p.PropItemId == id);

            if (proposal == null) return NotFound();

            return View(proposal);
        }

        // POST — does the actual work
        [HttpPost]
        [ActionName("ScheduleCollection")]  // ← tells ASP.NET this is still "ScheduleCollection"
        public IActionResult ScheduleCollectionConfirmed(int id)  // ← different method name
        {
            var proposal = db.ProposeItems.Find(id);

            if (proposal == null) return NotFound();

            bool anyDriversExist = db.DeliveryDrivers
                .Any(d => d.ApplicationStatus == Status.Approved);

            if (!anyDriversExist)
            {
                TempData["Error"] = "No drivers are registered in the system yet.";
                return RedirectToAction("AcceptedProposals");
            }

            var availableDriver = db.DeliveryDrivers
                .FirstOrDefault(d => d.IsAvailable == true &&
                                     d.ApplicationStatus == Status.Approved);

            if (availableDriver == null)
            {
                TempData["Error"] = "All drivers are currently busy. Please try again later.";
                return RedirectToAction("AcceptedProposals");
            }

            DateTime collectionDate = DateTime.Now.AddDays(4);

            proposal.CollectionDate = collectionDate;
            proposal.ProdStatus = Status.CollectionScheduled;
            proposal.AssignedDriverId = availableDriver.DriverId;

            availableDriver.IsAvailable = true;

            db.SaveChanges();

            TempData["Success"] = $"Collection scheduled for {collectionDate:dd MMM yyyy}. " +
                                  $"Driver {availableDriver.DriverName} " +
                                  $"{availableDriver.DriverSurname} has been assigned.";

            return RedirectToAction("AcceptedProposals");
        }

        // ── Add these to ShopOwnerController ──────────────────────────────────────────
        // Also inject PayStackTransferService via constructor:
        //
        //   private readonly PayStackTransferService _transfer;
        //   public ShopOwnerController(ApplicationDbContext context, PayStackTransferService transfer)
        //   { db = context; _transfer = transfer; }
        //
        // Register in Program.cs:
        //   builder.Services.AddScoped<PayStackTransferService>();

        // ── GET: show payment confirmation page ───────────────────────────────────────
        [HttpGet]
        public IActionResult MakePayment(int id)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var proposal = db.ProposeItems
                .Include(p => p.Customer)
                .Include(p => p.Checkout)
                .FirstOrDefault(p => p.PropItemId == id);

            if (proposal == null) return NotFound();

            if (proposal.Checkout == null)
            {
                TempData["Error"] = "Customer has not submitted their details yet.";
                return RedirectToAction("AcceptedProposals");
            }

            if (proposal.ProdStatus == Status.PaymentMade)
            {
                TempData["Error"] = "Payment has already been made for this proposal.";
                return RedirectToAction("AcceptedProposals");
            }

            return View(proposal);
        }

        // ── POST: process the PayStack transfer ───────────────────────────────────────
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ActionName("MakePayment")]  // ← tells ASP.NET this is still "MakePayment"

        //public async Task<IActionResult> MakeTranferPayment(int propItemId)
        //{
        //    try
        //    {
        //        int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

        //        var proposal = db.ProposeItems
        //            .Include(p => p.Customer)
        //            .Include(p => p.Checkout)
        //            .FirstOrDefault(p => p.PropItemId == propItemId);

        //        if (proposal == null) return NotFound();

        //        var checkout = proposal.Checkout;

        //        if (checkout == null)
        //        {
        //            TempData["Error"] = "Customer banking details are missing.";
        //            return RedirectToAction("AcceptedProposals");
        //        }
        //        // ── 1. Resolve bank code ──────────────────────────────────────────────────
        //        // Try branch code first (e.g. "051001"), fall back to bank name
        //        //string bankCode = await _transfer.ResolveBankCode(checkout.BranchCode)
        //        //               ?? await _transfer.ResolveBankCode(checkout.BankName);

        //        // ── 1. Bank code now comes straight from the database — no lookup needed ──
        //        string bankCode = checkout.BankCode;

        //        if (string.IsNullOrEmpty(bankCode))
        //        {
        //            TempData["Error"] = "Bank code is missing. Customer may need to re-submit their details.";
        //            return RedirectToAction("MakePayment", new { id = propItemId });
        //        }

        //        // ── 2. Create transfer recipient ──────────────────────────────────────────
        //        var recipient = await _transfer.CreateRecipient(
        //            name: checkout.AccountHolderName,
        //            accountNumber: checkout.AccountNumber,
        //            bankCode: bankCode
        //        );

        //        //// ── 2. Create transfer recipient ──────────────────────────────────────────
        //        //var recipient = await _transfer.CreateRecipient(
        //        //    name: checkout.AccountHolderName,
        //        //    accountNumber: checkout.AccountNumber,
        //        //    bankCode: bankCode
        //        //);

        //        if (!recipient.Success)
        //        {
        //            TempData["Error"] = "Could not create payment recipient: " + recipient.Message;
        //            return RedirectToAction("MakePayment", new { id = propItemId });
        //        }

        //        // ── 3. Initiate the transfer ──────────────────────────────────────────────
        //        string reference = $"PROP-{propItemId}-{DateTime.Now.Ticks}";
        //        string reason = $"Payment for item: {proposal.ProdName}";

        //        var transfer = await _transfer.InitiateTransfer(
        //            recipientCode: recipient.RecipientCode,
        //            amountRand: proposal.AmountProposed,
        //            reason: reason,
        //            reference: reference
        //        );

        //        if (!transfer.Success)
        //        {
        //            TempData["Error"] = "Transfer failed: " + transfer.Message;
        //            return RedirectToAction("MakePayment", new { id = propItemId });
        //        }

        //        // ── 4. Mark proposal as paid ──────────────────────────────────────────────
        //        proposal.ProdStatus = Status.PaymentMade;
        //        db.SaveChanges();

        //        TempData["Success"] = $"Payment of R {proposal.AmountProposed:N2} sent to {checkout.AccountHolderName}.";
        //        TempData["TransferCode"] = transfer.TransferCode;
        //        TempData["Reference"] = reference;

        //        return RedirectToAction("AcceptedProposals");

        //    }
        //    catch (Exception ex)
        //    {
        //        // Temporarily show the full error
        //        TempData["Error"] = "Exception: " + ex.Message + " | " + ex.InnerException?.Message;
        //        return RedirectToAction("MakePayment", new { id = propItemId });
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("MakePayment")]
        public IActionResult MakeTranferPayment(int propItemId)
        {
            var proposal = db.ProposeItems
                .Include(p => p.Customer)
                .Include(p => p.Checkout)
                .FirstOrDefault(p => p.PropItemId == propItemId);

            if (proposal == null) return NotFound();

            if (proposal.ProdStatus == Status.PaymentMade)
            {
                TempData["Error"] = "Payment has already been made for this proposal.";
                return RedirectToAction("AcceptedProposals");
            }

            // Mark as paid
            proposal.ProdStatus = Status.PaymentMade;
            db.SaveChanges();

            // Pass details to the view via TempData so the success popup can display them
            TempData["PaymentSuccess"] = "true";
            TempData["PaidAmount"] = proposal.AmountProposed.ToString("N2");
            TempData["PaidToName"] = proposal.Checkout?.AccountHolderName ?? "Customer";
            TempData["PaidProductName"] = proposal.ProdName;

            return RedirectToAction("AcceptedProposals");
        }



        //----------------------------------------------------------------------------------CUTTED
        public IActionResult JobCenter()
        {
            return View("ManagingJobs/JobCenter");
        }

        public IActionResult ComposeJob()
        {
            return View("Managingjobs/ComposeJob");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ComposeJob(JobPost model)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;
            //validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return Content(string.Join("\n", errors));
            }
            //if (!ModelState.IsValid)
            //{
            //    return View("ManagingJobs/ComposeJob", model);
            //    //return View(model);
            //}

            model.OwnerId = currentId;
            model.PostStatus = CurrentPostStatus.Published;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            db.Add(model);
            await db.SaveChangesAsync();
            TempData["successful"] = "Job Posted successfully";
            return RedirectToAction("JobCenter");
           
        }


        // ---------- AI-ASSISTED POST ----------

        // Step 1: owner fills the short "seed" form
        //public IActionResult ComposeJobAI()
        //{
        //    return View("ManagingJobs/ComposeJobAI");
        //}
       

        public async Task<IActionResult> ComposeJobAI(int roleId)
        {
            var role = await db.JobRoleKnowledges.FindAsync(roleId);
            if (role == null) return NotFound();

            ViewBag.RoleId = role.RoleId;
            ViewBag.JobTitle = role.JobTitle;
            return View("ManagingJobs/ComposeJobAI");
        }

  
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> GenerateJobDraft(int roleId, JobPost model)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;
            var role = await db.JobRoleKnowledges.FindAsync(roleId);
            if (role == null) return NotFound();

            JobPostDraftResult draft;
            try
            {
                draft = await _aiService.GenerateDraftFromRoleAsync(role);
            }
            catch
            {
                draft = new JobPostDraftResult
                {
                    Responsibilities = role.CompanyResponsibilities,
                    RequiredCerts = role.RequiredQualifications,
                    RequiredSkillsAndXP = role.RequiredSkills + "\n" + role.RequiredExperience
                };
                TempData["aiError"] = "AI formatting failed — showing the raw approved information instead.";
            }

            model.OwnerId = currentId;
            model.JobTitle = role.JobTitle;
            model.Responsibilities = draft.Responsibilities;
            model.RequiredCerts = draft.RequiredCerts;
            model.RequiredSkillsAndXP = draft.RequiredSkillsAndXP;
            model.PostStatus = CurrentPostStatus.Draft;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            db.Add(model);
            await db.SaveChangesAsync();
            return RedirectToAction("ReviewJobDraft", new { id = model.PostId });
        }
        // Step 3: owner reviews/edits the AI draft
        [HttpGet]
        public async Task<IActionResult> ReviewJobDraft(int id)
        {
            var post = await db.JobPosts.FindAsync(id);
            if (post == null) return NotFound();
            return View("ManagingJobs/ReviewJobDraft", post);
        }

        // Step 4a: owner edits text then clicks Publish
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> PublishJobDraft(JobPost model)
        {
            var post = await db.JobPosts.FindAsync(model.PostId);
            if (post == null) return NotFound();

            post.Responsibilities = model.Responsibilities;
            post.RequiredCerts = model.RequiredCerts;
            post.RequiredSkillsAndXP = model.RequiredSkillsAndXP;
            post.PostStatus = CurrentPostStatus.Published;
            post.UpdatedAt = DateTime.Now;

            await db.SaveChangesAsync();
            TempData["successful"] = "Job Posted successfully";
            return RedirectToAction("JobCenter");
        }

        // Step 4b: owner isn't happy with the draft, asks AI to try again
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RegenerateJobDraft(int id)
        {
            var post = await db.JobPosts.FindAsync(id);
            if (post == null) return NotFound();

            var draft = await _aiService.GenerateDraftAsync(new JobPostDraftRequest
            {
                JobTitle = post.JobTitle,
                Department = post.Department.ToString(),
                Location = post.Location.ToString(),
                EmploymentType = post.EmploymentType.ToString(),
                SalaryMin = post.SalaryMin,
                SalaryMax = post.SalaryMax
            });

            post.Responsibilities = draft.Responsibilities;
            post.RequiredCerts = draft.RequiredCerts;
            post.RequiredSkillsAndXP = draft.RequiredSkillsAndXP;
            post.UpdatedAt = DateTime.Now;

            await db.SaveChangesAsync();
            return RedirectToAction("ReviewJobDraft", new { id });
        }

        // GET: dropdown of active roles
        public IActionResult SelectJobRole()
        {
            var roles = db.JobRoleKnowledges
                .Where(r => r.IsActive)
                .OrderBy(r => r.JobTitle)
                .ToList();

            ViewBag.Roles = new SelectList(roles, "RoleId", "JobTitle");
            return View("ManagingJobs/SelectJobRole");
        }

        // POST: owner picked a role and a mode (manual or AI)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectJobRole(int roleId, string mode)
        {
            var role = await db.JobRoleKnowledges.FindAsync(roleId);
            if (role == null) return NotFound();

            if (mode == "manual")
            {
                // prefill the existing manual ComposeJob form with approved info
                var seeded = new JobPost
                {
                    JobTitle = role.JobTitle,
                    Responsibilities = role.CompanyResponsibilities,
                    RequiredCerts = role.RequiredQualifications,
                    RequiredSkillsAndXP = role.RequiredSkills + "\n" + role.RequiredExperience
                };
                return View("ManagingJobs/ComposeJob", seeded);
            }

            // AI mode — go collect the remaining seed fields (location, salary, etc.)
            return RedirectToAction("ComposeJobAI", new { roleId = role.RoleId });
        }







        // View system-recommended job posts (for ShopOwner)
        [HttpGet]
        public async Task<IActionResult> SystemRecommendedPosts()
        {
            var recommendations = await db.JobVacancyRecommendations
                .Where(r => r.Status == RecommendationStatus.Draft)  // Only drafts, not yet published
                .Include(r => r.Department)
                .Include(r => r.Position)
                .Include(r => r.LeaveRequest)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync();

            return View(recommendations);
        }

        // Review a system-recommended post
        [HttpGet]
        public async Task<IActionResult> ReviewRecommendation(int id)
        {
            var rec = await db.JobVacancyRecommendations
                .Include(r => r.Department)
                .Include(r => r.Position)
                .Include(r => r.LeaveRequest)
                .FirstOrDefaultAsync(r => r.Id == id && r.Status == RecommendationStatus.Draft);

            if (rec == null)
                return NotFound();

            return View(rec);
        }

        // Edit recommendation details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRecommendation(int id, string SuggestedJobTitle, string SuggestedResponsibilities,
            string SuggestedRequiredCerts, string SuggestedRequiredSkillsAndXP)
        {
            var rec = await db.JobVacancyRecommendations.FindAsync(id);
            if (rec == null)
                return NotFound();

            rec.SuggestedJobTitle = SuggestedJobTitle;
            rec.SuggestedResponsibilities = SuggestedResponsibilities;
            rec.SuggestedRequiredCerts = SuggestedRequiredCerts;
            rec.SuggestedRequiredSkillsAndXP = SuggestedRequiredSkillsAndXP;

            await db.SaveChangesAsync();

            TempData["success"] = "Recommendation updated successfully.";
            return RedirectToAction("ReviewRecommendation", new { id });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishRecommendation(int id)
        {
            var rec = await db.JobVacancyRecommendations
                .Include(r => r.Department)
                .Include(r => r.Position)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rec == null)
                return NotFound();

            int? ownerId = HttpContext.Session.GetInt32("OwnerId");
            if (!ownerId.HasValue)
            {
                TempData["error"] = "Shop owner not found in session.";
                return RedirectToAction("ReviewRecommendation", new { id });
            }

            try
            {
                // Map Department to SelectDepartment enum
                // You may need to adjust this mapping based on your department names
                SelectDepartment department = MapDepartmentToEnum(rec.Department.Name);

                // Map VacancyType to EmploymentType
                SelectEmploymentType employmentType = rec.VacancyType == VacancyType.Permanent
                    ? SelectEmploymentType.FullTime
                    : SelectEmploymentType.Part_Time;

                // Create JobPost from recommendation
                var jobPost = new JobPost
                {
                    OwnerId = ownerId.Value,
                    JobTitle = rec.SuggestedJobTitle,
                    Department = department,
                    Location = SelectLocation.Durban,  // Default to Durban, can be changed by user later
                    EmploymentType = employmentType,
                    Responsibilities = rec.SuggestedResponsibilities,
                    RequiredCerts = rec.SuggestedRequiredCerts,
                    RequiredSkillsAndXP = rec.SuggestedRequiredSkillsAndXP,
                    ClosingDate = rec.VacancyType == VacancyType.TemporaryLeaveCover
                        ? rec.ContractEndDate?.AddDays(1)
                        : DateTime.Now.AddDays(30),
                    PostStatus = CurrentPostStatus.Published,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.JobPosts.Add(jobPost);

                // Mark recommendation as published
                rec.Status = RecommendationStatus.Published;
                rec.PublishedAt = DateTime.UtcNow;
                rec.ReviewedByStaffId = HttpContext.Session.GetInt32("UserId");
                rec.ReviewedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                TempData["success"] = "Job post published successfully!";
                return RedirectToAction("ReviewRecommendation", new { id });
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error publishing: {ex.Message}";
                return RedirectToAction("ReviewRecommendation", new { id });
            }
        }

        // Helper method to map Department name to SelectDepartment enum
        private SelectDepartment MapDepartmentToEnum(string departmentName)
        {
            return departmentName.ToLower() switch
            {
                "sales" => SelectDepartment.Sales_floor,
                "hr" or "human resources" => SelectDepartment.Management,  // Adjust as needed
                "it" or "technician" => SelectDepartment.Technician,
                "management" => SelectDepartment.Management,
                _ => SelectDepartment.Sales_floor  // Default fallback
            };
        }

        // Delete recommendation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecommendation(int id)
        {
            var rec = await db.JobVacancyRecommendations.FindAsync(id);
            if (rec == null)
                return NotFound();

            db.JobVacancyRecommendations.Remove(rec);
            await db.SaveChangesAsync();

            TempData["success"] = "Recommendation deleted.";
            return RedirectToAction("SystemRecommendedPosts");
        }

    }
}
