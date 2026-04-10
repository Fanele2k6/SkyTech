// ============================================================
// DeliveryDriverController.cs — fully fixed
// ============================================================
using CM_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            int currentId = HttpContext.Session.GetInt32("UserId").Value;

            var inProgress = db.Orders
                .Where(o => o.DriverId == currentId && o.Status == OrderStatus.OutForDelivery)
                .Count();

            var completed = db.Orders
                .Where(o => o.DriverId == currentId && o.Status == OrderStatus.Delivered)
                .Count();

            ViewBag.InProgressDeliveries = inProgress;
            ViewBag.CompletedDeliveries = completed;
            return View();
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
            order.DeliveredOn = DateTime.Now;
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
        //public IActionResult CompleteDelivery(int id)
        //{
        //    var order = db.Orders.Find(id);

        //    if (order == null)
        //        return Json(new { success = false, reason = "Order not found" });

        //    if (order.Status != OrderStatus.OutForDelivery)
        //        return Json(new { success = false, reason = "Order is not out for delivery" });

        //    order.Status = OrderStatus.Delivered;
        //    order.DeliveredOn = DateTime.Now;

        //    var driver = db.DeliveryDrivers.Find(order.DriverId);
        //    if (driver != null) driver.IsAvailable = true;

        //    db.SaveChanges();

        //    return Json(new { success = true });
        //}

        //Delivery pin action method
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

        public IActionResult ReturnedOrderItems()
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var AsssignedReturns = db.OrderItems
                .Include(i => i.Order)
                    .ThenInclude(o => o.Customers)
                .Include(i => i.Products)
                    .ThenInclude(p => p.ProductPictures)
                .Where(i => i.ReturnPStatus == ReturnProduct.Return_Approved
                         && i.Order.OwnerId == currentId)   // ← only this shop's orders
                .OrderByDescending(i => i.Order.DeliveredOn)
                .ToList();

            return View(AsssignedReturns);
        }
    }
}
