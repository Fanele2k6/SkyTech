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

            if (order.Status != OrderStatus.Assigned)
                return Json(new { success = false, reason = "Order is not in Assigned state" });

            order.Status = OrderStatus.OutForDelivery;
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult CompleteDelivery(int id)
        {
            var order = db.Orders.Find(id);

            if (order == null)
                return Json(new { success = false, reason = "Order not found" });

            if (order.Status != OrderStatus.OutForDelivery)
                return Json(new { success = false, reason = "Order is not out for delivery" });

            order.Status = OrderStatus.Delivered;
            order.DeliveredOn = DateTime.Now;

            var driver = db.DeliveryDrivers.Find(order.DriverId);
            if (driver != null) driver.IsAvailable = true;

            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
