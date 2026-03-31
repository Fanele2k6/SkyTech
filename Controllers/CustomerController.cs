using CM_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static CM_Project.Models.Order;

namespace CM_Project.Controllers
{
    [AuthorizeUser]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext db;

        public CustomerController(ApplicationDbContext context) { db = context; }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ScanProduct(string barcode)
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;

            DeleteExpiredOrders(currentId);

            if (!string.IsNullOrEmpty(barcode))
                return Redirect("https://www.google.com/search?q=" + barcode);

            TempData["Error"] = "No barcode entered.";
            return RedirectToAction("ScanProduct");
        }

        [HttpGet]
        public IActionResult ShoppingPage(string search, SelectCategory? category)
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;
            DeleteExpiredOrders(currentId);

            var products = db.AddProducts
                .Include(p => p.ProductPictures)
                .Include(p => p.Owner)
                .AsQueryable()
                .Take(10);

            var popularPicks = db.OrderItems
                .GroupBy(o => o.ProdId)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .Join(db.AddProducts, p => p.ProductId, prod => prod.ProdId, (p, prod) => prod)
                .ToList();

            var category1 = db.AddProducts
                .Include(p => p.ProductPictures)
                .Include(p => p.Owner)
                .Where(p => p.ProdCategory == SelectCategory.Groceries)
                .Take(10)
                .ToList();

            if (category.HasValue)
                products = products.Where(p => p.ProdCategory == category.Value);

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.ProdName.Contains(search));

            var cartNotification = db.Carts
                .Where(c => c.CustomerId == currentId && !c.IsCheckedOut)
                .SelectMany(c => c.CartItems)
                .Count();

            ViewBag.PopularPicks = popularPicks;
            ViewBag.Groceries = category1;
            ViewBag.NumberOfCartItems = cartNotification;
            ViewBag.SelectedCategory = category;
            ViewBag.Search = search;

            return View(products.ToList());
        }

        public IActionResult AddToCart(int id, int quantity)
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;
            var product = db.AddProducts.Find(id);

            if (product == null || product.Status == ProdStatus.Out_Of_Stock)
                return Content("No products / Out of stock");

            var cart = db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.CustomerId == currentId && !c.IsCheckedOut);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = currentId,
                    DateCreated = DateTime.Now,
                    IsCheckedOut = false,
                    CartItems = new List<CartItem>()
                };
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProdId == id);
            if (existingItem != null)
                existingItem.ProdQuantity += quantity;
            else
                cart.CartItems.Add(new CartItem
                {
                    ProdId = id,
                    ProdName = product.ProdName,
                    ProdQuantity = quantity,
                    PriceAtTimeAdded = product.ProdPrice
                });

            db.SaveChanges();
            return RedirectToAction("ShoppingPage", "Customer");
        }

        public IActionResult ViewCart()
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;
            DeleteExpiredOrders(currentId);

            var address = db.Customers
                .Where(c => c.CustomerId == currentId)
                .Select(c => c.Address)
                .FirstOrDefault();

            var itemsInCart = db.Carts
                .Where(c => c.CustomerId == currentId && !c.IsCheckedOut)
                .SelectMany(c => c.CartItems)
                .Count();

            // ✅ ThenInclude loads Products AND ProductPictures
            var cart = db.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Products)
                        .ThenInclude(p => p.ProductPictures)
                .FirstOrDefault(c => c.CustomerId == currentId && !c.IsCheckedOut);

            ViewBag.Address = address;
            ViewBag.ItemsInCart = itemsInCart;

            if (cart == null)
                return View(new List<CartItem>());

            return View(cart.CartItems.ToList());
        }

        public IActionResult RemoveFromCart() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int id)
        {
            var item = db.CartItems.FirstOrDefault(c => c.CartItemId == id);
            if (item != null) { db.CartItems.Remove(item); db.SaveChanges(); }
            return RedirectToAction("ViewCart", "Customer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(int id, int quantity)
        {
            var item = db.CartItems.FirstOrDefault(c => c.CartItemId == id);
            if (item != null)
            {
                if (quantity <= 0) db.CartItems.Remove(item);
                else item.ProdQuantity = quantity;
                db.SaveChanges();
            }
            return RedirectToAction("ViewCart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(string deliveryAddress)
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;

            // ✅ Load cart with Products included
            var cart = db.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Products)
                .FirstOrDefault(c => c.CustomerId == currentId && !c.IsCheckedOut);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("ViewCart");
            }

            // ✅ Null-safe: fall back to stored price if Products is null
            double totalAmount = cart.CartItems.Sum(ci =>
                (ci.Products != null ? ci.Products.ProdPrice : ci.PriceAtTimeAdded) * ci.ProdQuantity);

            if (!string.IsNullOrWhiteSpace(deliveryAddress))
            {
                db.Database.ExecuteSqlRaw(
                    "UPDATE Customers SET Address = {0} WHERE CustomerId = {1}",
                    deliveryAddress.Trim(), currentId);
            }

            // ✅ Include Products so stock check doesn't crash
            var cartItems = db.CartItems
                .Include(c => c.Products)
                .Where(c => c.Carts.CustomerId == currentId)
                .ToList();

            var address = db.Customers
                .Where(a => a.CustomerId == currentId)
                .Select(a => a.Address)
                .FirstOrDefault();

            var customer = db.Customers.FirstOrDefault(c => c.CustomerId == currentId);

            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("ViewCart");
            }

            foreach (var item in cartItems)
            {
                var product = db.AddProducts.Find(item.ProdId);
                if (product == null) continue;

                if (product.ProdQuantity < item.ProdQuantity)
                {
                    TempData["Error"] = product.ProdName + " does not have enough stock available.";
                    return RedirectToAction("ViewCart");
                }

                product.ProdQuantity -= item.ProdQuantity;
                product.Status = product.ProdQuantity == 0 ? ProdStatus.Out_Of_Stock
                               : product.ProdQuantity <= 5 ? ProdStatus.Low_stock
                                                            : ProdStatus.In_Stock;
            }

            double deliveryFee = 50;
            double subtotal = cartItems.Sum(i => i.SubTotal);
            double total = subtotal + deliveryFee;

            var order = new Order
            {
                CustomerId = currentId,
                OrderedOn = DateTime.Now,
                Status = OrderStatus.Pending,
                PaymentExpiry = DateTime.Now.AddMinutes(2),
                TotalAmount = totalAmount,
                DeliveryAddress = string.IsNullOrWhiteSpace(deliveryAddress)
                                    ? address
                                    : deliveryAddress.Trim(),
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProdId = ci.ProdId,
                    ProdName = ci.ProdName,
                    Quantity = ci.ProdQuantity,
                    PriceAtTimeOfPurchase = ci.PriceAtTimeAdded
                }).ToList()
            };

            db.Orders.Add(order);
            db.SaveChanges();

            return RedirectToAction("PaymentPage", "Payment", new
            {
                orderId = order.OrderId,
                amount = total,
                customerEmail = customer.Email,
                customerName = customer.FirstName,
                itemCount = cart.CartItems.Sum(i => i.ProdQuantity),
                deliveryAddress
            });
        }

        private void DeleteExpiredOrders(int customerId)
        {
            var expiredOrders = db.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == customerId
                         && !o.IsPaid
                         && o.PaymentExpiry < DateTime.Now)
                .ToList();

            foreach (var order in expiredOrders)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = db.AddProducts.Find(item.ProdId);
                    if (product != null) product.ProdQuantity += item.Quantity;
                }
                db.OrderItems.RemoveRange(order.OrderItems);
                db.Orders.Remove(order);
            }
            db.SaveChanges();
        }

        public IActionResult OrderHistory()
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;
            DeleteExpiredOrders(currentId);

            var orders = db.Orders
                .Where(o => o.CustomerId == currentId && o.IsPaid)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Products)
                        .ThenInclude(p => p.Owner)
                .OrderByDescending(o => o.OrderedOn)
                .ToList();

            return View(orders);
        }

        public IActionResult CustomerProfile()
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;
            DeleteExpiredOrders(currentId);
            return View(db.Customers.Where(c => c.CustomerId == currentId).ToList());
        }

        [HttpGet]
        public IActionResult TrackOrder(int? id)
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null)
            {
                ViewBag.Message = "Please log in to track your order.";
                return View((CM_Project.Models.Order)null);
            }

            DeleteExpiredOrders(customerId.Value);

            Order order;
            if (id == null)
            {
                order = db.Orders
                    .Include(o => o.Driver)
                    .Include(o => o.OrderItems)
                    .Where(o => o.CustomerId == customerId && !o.IsArchived)
                    .OrderByDescending(o => o.OrderedOn)
                    .FirstOrDefault();

                if (order == null)
                {
                    ViewBag.Message = "You don't have any active orders at the moment.";
                    return View((CM_Project.Models.Order)null);
                }
            }
            else
            {
                order = db.Orders
                    .Include(o => o.Driver)
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.OrderId == id.Value
                                     && o.CustomerId == customerId
                                     && !o.IsArchived);

                if (order == null)
                {
                    ViewBag.Message = "You don't have any active orders at the moment.";
                    return View((CM_Project.Models.Order)null);
                }
            }

            ViewBag.StatusMessage = GetStatusMessage(order.Status);
            return View(order);
        }

        private string GetStatusMessage(CM_Project.Models.OrderStatus status) =>
            status == CM_Project.Models.OrderStatus.Pending ? "Pending"
          : status == CM_Project.Models.OrderStatus.Assigned ? "Driver Assigned"
          : status == CM_Project.Models.OrderStatus.OutForDelivery ? "Out For Delivery"
          : status == CM_Project.Models.OrderStatus.Delivered ? "Delivered"
          : "";

        [HttpGet]
        public IActionResult GetOrderStatus(int orderId)
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            int step = order.Status == CM_Project.Models.OrderStatus.Pending ? 0
                     : order.Status == CM_Project.Models.OrderStatus.Assigned ? 1
                     : order.Status == CM_Project.Models.OrderStatus.OutForDelivery ? 2
                     : order.Status == CM_Project.Models.OrderStatus.Delivered ? 3 : 0;

            return Json(new { currentStep = step });
        }

        [HttpPost]
        public IActionResult ArchiveOrder(int orderId)
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null)
                return Json(new { success = false, reason = "Not logged in" });

            var order = db.Orders
                .FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == customerId);

            if (order == null)
                return Json(new { success = false, reason = "Order not found" });

            if (order.Status != CM_Project.Models.OrderStatus.Delivered)
                return Json(new { success = false, reason = "Order not delivered yet" });

            order.IsArchived = true;
            order.DeliveredOn = order.DeliveredOn ?? DateTime.Now;
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult MyOrders()
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null) return RedirectToAction("LogIn", "Authentication");

            DeleteExpiredOrders(customerId.Value);

            var orders = db.Orders
                .Include(o => o.Driver)
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderedOn)
                .ToList();

            return View(orders);
        }
    }
}
