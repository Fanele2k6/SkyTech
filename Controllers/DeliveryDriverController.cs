// ============================================================
// DeliveryDriverController.cs — fully fixed
// ============================================================
using CM_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyTech.Models;
using System;
using System.Linq;

namespace CM_Project.Controllers
{
    [AuthorizeUser]
    public class DeliveryDriverController : Controller
    {
        private readonly ApplicationDbContext db;

        public DeliveryDriverController(ApplicationDbContext context) { db = context; }

        [HttpGet]
        public IActionResult DriverDash()
        {
            int driverId = HttpContext.Session.GetInt32("UserId")!.Value;

            // Get driver info
            var driver = db.DeliveryDrivers.Find(driverId);
            ViewBag.DriverName = driver?.DriverName + " " + driver?.DriverSurname;
            ViewBag.ApplicationStatus = driver?.ApplicationStatus;
            ViewBag.ActivePage = "Dashboard";

            // Get stats
            ViewBag.CompletedDeliveries = db.Orders.Count(o => o.DriverId == driverId && o.Status == OrderStatus.Delivered);
            ViewBag.InProgressDeliveries = db.Orders.Count(o => o.DriverId == driverId && (o.Status == OrderStatus.Assigned || o.Status == OrderStatus.OutForDelivery));

            return View(driver);
        }

        // ✅ ThenInclude Products so view doesn't crash
        public IActionResult DriverDeliveryView(int id)
        {
            var order = db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Products)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpGet]
        public IActionResult MyDeliveries()
        {
            int driverId = HttpContext.Session.GetInt32("UserId").Value;

            // ✅ ThenInclude Products
            var deliveries = db.Orders
                .Include(o => o.Customers)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Products)
                .Where(o => o.DriverId == driverId)
                .OrderByDescending(o => o.OrderedOn)
                .ToList();

            ViewBag.DriverId = driverId;

            return View(deliveries);
        }

        [HttpPost]
        public IActionResult StartDelivery(int id)
        {
            var order = db.Orders.Find(id);

            if (order == null)
                return Json(new { success = false, reason = "Order not found" });

            // Block if today is before the scheduled delivery date
            if (DateTime.Today < order.ScheduledDeliveryDate.Date)
                return Json(new
                {
                    success = false,
                    reason = "This delivery is not scheduled for today. Scheduled for "
                    + order.ScheduledDeliveryDate.ToString("dddd, dd MMM yyyy")
                });

            order.Status = OrderStatus.OutForDelivery;
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteDelivery(int id, int driverId)
        {
            var order = db.Orders.Find(id);
            if (order == null)
                return Json(new { success = false, reason = "Order not found" });

            // Mark this order as delivered
            order.Status = OrderStatus.Delivered;
            db.SaveChanges();

            var driver = db.DeliveryDrivers.Find(order.DriverId);
            if (driver != null)
            {
                // Count remaining undelivered orders for this driver
                // on the same scheduled delivery date
                int remainingOrdersToday = db.Orders.Count(o =>
                    o.DriverId == driver.DriverId &&
                    o.ScheduledDeliveryDate.Date == order.ScheduledDeliveryDate.Date &&
                    o.Status != OrderStatus.Delivered);

                // Only make driver available again when ALL orders for that day are done
                if (remainingOrdersToday == 0)
                    driver.IsAvailable = true;

                db.SaveChanges();
            }

            return Json(new { success = true });
        }
        
        [HttpPost]
        public IActionResult VerifyDeliveryPin(int orderId, string pin)
        {
            var order = db.Orders.Find(orderId);
            if (order == null)
                return Json(new { success = false, reason = "Order not found" });

            if (order.DeliveryPin != pin.Trim())
                return Json(new { success = false, reason = "Incorrect pin" });

            return Json(new { success = true });
        }


        //Driver transfers
        public IActionResult MyTransfers()
        {
            int driverId = HttpContext.Session.GetInt32("UserId")!.Value;

            var transfers = db.InterBranchTransfers
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Products)
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                        .ThenInclude(o => o.Customers)
                .Include(t => t.SourceBranch)
                .Include(t => t.TargetBranch)
                .Where(t => t.DriverId == driverId)
                .OrderBy(t => t.ScheduledTransferDate)
                .ToList();

            ViewBag.DriverName = HttpContext.Session.GetString("DriverName");
            return View(transfers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkTransferInTransit(int transferId)
        {
            int driverId = HttpContext.Session.GetInt32("UserId")!.Value;

            var transfer = db.InterBranchTransfers
                .FirstOrDefault(t => t.TransferId == transferId && t.DriverId == driverId);

            if (transfer == null)
            {
                TempData["Error"] = "Transfer not found or not assigned to you.";
                return RedirectToAction("MyTransfers");
            }

            transfer.TransferStatus = ItemTransferStatus.InTransit;
            db.SaveChanges();

            TempData["Success"] = "Transfer marked as in transit. Please deliver to the destination branch.";

            return RedirectToAction("MyTransfers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkTransferDelivered(int transferId)
        {
            int driverId = HttpContext.Session.GetInt32("UserId")!.Value;

            var transfer = db.InterBranchTransfers
                .Include(t => t.OrderItem)
                .FirstOrDefault(t => t.TransferId == transferId && t.DriverId == driverId);

            if (transfer == null)
            {
                TempData["Error"] = "Transfer not found or not assigned to you.";
                return RedirectToAction("MyTransfers");
            }

            transfer.TransferStatus = ItemTransferStatus.DeliveredToPickupBranch;
            transfer.DeliveredToTargetOn = DateTime.Now;
            transfer.IsDelivered = true;
            db.SaveChanges();

            TempData["Success"] = $"Item {transfer.OrderItem?.ProdName} has been delivered to the pickup branch.";

            return RedirectToAction("MyTransfers");
        }


        //MY DELIVERIES
        public IActionResult HomeDeliveries()
        {
            int driverId = HttpContext.Session.GetInt32("UserId")!.Value;

            var deliveries = db.Orders
                .Include(o => o.Customers)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Products)
                .Where(o => o.DriverId == driverId && o.OrderType == DeliveryType.HomeDelivery)
                .OrderByDescending(o => o.OrderedOn)
                .ToList();

            ViewBag.DriverId = driverId;
            ViewBag.DriverName = HttpContext.Session.GetString("DriverName");
            ViewBag.ActivePage = "HomeDeliveries";

            return View(deliveries);
        }


        public IActionResult BranchTransfers()
        {
            int driverId = HttpContext.Session.GetInt32("UserId")!.Value;

            var transfers = db.InterBranchTransfers
                    .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Products)
                    .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                        .ThenInclude(o => o.Customers)
                .Include(t => t.SourceBranch)
                .Include(t => t.TargetBranch)
                .Where(t => t.DriverId == driverId)
                .OrderBy(t => t.ScheduledTransferDate)
                .ToList();

            ViewBag.DriverName = HttpContext.Session.GetString("DriverName");
            ViewBag.ActivePage = "BranchTransfers";

            return View(transfers);
        }

    }
}
