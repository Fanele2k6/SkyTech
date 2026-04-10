using CM_Project.Models;
using CM_Project.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SkyTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static CM_Project.Models.Order;

namespace CM_Project.Controllers
{
   // [AuthorizeUser]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext db;

        public CustomerController(ApplicationDbContext context) { db = context; }


        public IActionResult ProductDetails(int id)  // ← receive product id from URL
        {
            var product = db.AddProducts
                .Include(p => p.ProductPictures)
                .Include(p => p.Owner)
                .FirstOrDefault(p => p.ProdId == id);  // ← use product id, not userId

            if (product == null) return NotFound();

            return View(product);  // single product, not a list
        }

        [HttpGet]
        public IActionResult ShoppingPage(string search, SelectCategory? category)
        {
            // Make currentId nullable — guest users have no session
            int? currentId = HttpContext.Session.GetInt32("UserId");

            // Only delete expired orders if logged in
            if (currentId.HasValue)
                DeleteExpiredOrders(currentId.Value);

            var products = db.AddProducts
                .Include(p => p.ProductPictures)
                .Include(p => p.Owner)
                .AsQueryable()
                .Take(10);

            var topProductIds = db.OrderItems
                .GroupBy(o => o.ProdId)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .Select(x => x.ProductId)
                .ToList();

            var popularPicks = db.AddProducts
                .Include(p => p.ProductPictures)
                .Include(p => p.Owner)
                .Where(p => topProductIds.Contains(p.ProdId))
                .ToList();

            var category1 = db.AddProducts
                .Include(p => p.ProductPictures)
                .Include(p => p.Owner)
                .Where(p => p.ProdCategory == SelectCategory.Appliances)
                .Take(10)
                .ToList();

            if (category.HasValue)
                products = products.Where(p => p.ProdCategory == category.Value);

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.ProdName.Contains(search));

            // Only load cart count if logged in, otherwise show 0
            var cartNotification = currentId.HasValue
                ? db.Carts
                    .Where(c => c.CustomerId == currentId.Value && !c.IsCheckedOut)
                    .SelectMany(c => c.CartItems)
                    .Count()
                : 0;

            ViewBag.PopularPicks = popularPicks;
            ViewBag.Groceries = category1;
            ViewBag.NumberOfCartItems = cartNotification;
            ViewBag.SelectedCategory = category;
            ViewBag.Search = search;

            return View(products.ToList());
        }


       
        public IActionResult AddToCart(int id, int quantity)
        {
         //   int currentId= HttpContext.Session.GetInt32("UserId").Value;
            var product = db.AddProducts.Find(id);
            if (product == null || product.Status == ProdStatus.Out_Of_Stock)
                return Content("No products / Out of stock");

            int? sessionId = HttpContext.Session.GetInt32("UserId");

            //Allow guest users to add to cart
            if (sessionId == null)
            {
                // Guest — store cart in session as JSON
                var guestCart = HttpContext.Session.GetString("GuestCart");
                var cartItems = string.IsNullOrEmpty(guestCart)
                    ? new List<GuestCartItem>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<GuestCartItem>>(guestCart);

                var existing = cartItems.FirstOrDefault(c => c.ProdId == id);
                if (existing != null)
                    existing.Quantity += quantity;
                else
                    cartItems.Add(new GuestCartItem
                    {
                        ProdId = id,
                        ProdName = product.ProdName,
                        Quantity = quantity,
                        Price = product.ProdPrice
                    });

                HttpContext.Session.SetString("GuestCart",
                    System.Text.Json.JsonSerializer.Serialize(cartItems));

                return RedirectToAction("ShoppingPage", "Customer");
            }

            int currentId = sessionId.Value;
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

        //  [AuthorizeUser]

        public IActionResult ViewCart()
        {
            int? sessionId = HttpContext.Session.GetInt32("UserId");

            if (sessionId == null)
            {
                var guestCart = HttpContext.Session.GetString("GuestCart");
                var guestItems = string.IsNullOrEmpty(guestCart)
                    ? new List<GuestCartItem>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<GuestCartItem>>(guestCart);

                var cartItems = guestItems.Select(g => new CartItem
                {
                    ProdId = g.ProdId,
                    ProdName = g.ProdName,
                    ProdQuantity = g.Quantity,
                    PriceAtTimeAdded = g.Price
                }).ToList();

                // Load product pictures for guest cart items
                var prodIds = guestItems.Select(g => g.ProdId).ToList();
                var guestPictures = db.AddProducts
                    .Include(p => p.ProductPictures)
                    .Where(p => prodIds.Contains(p.ProdId))
                    .ToDictionary(p => p.ProdId, p => p.ProductPictures?.FirstOrDefault()?.FilePath);

                ViewBag.GuestPictures = guestPictures; // Dictionary<int, string>
                ViewBag.IsGuest = true;
                ViewBag.Address = "";
                ViewBag.ItemsInCart = cartItems.Count;

                return View(cartItems);
            }

            int currentId = sessionId.Value;
            DeleteExpiredOrders(currentId);

            var address = db.Customers
                .Where(c => c.CustomerId == currentId)
                .Select(c => c.Address)
                .FirstOrDefault();

            var itemsInCart = db.Carts
                .Where(c => c.CustomerId == currentId && !c.IsCheckedOut)
                .SelectMany(c => c.CartItems)
                .Count();

            var cart = db.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.AddProduct)
                        .ThenInclude(p => p.ProductPictures)
                .FirstOrDefault(c => c.CustomerId == currentId && !c.IsCheckedOut);

            ViewBag.IsGuest = false;
            ViewBag.Address = address;
            ViewBag.ItemsInCart = itemsInCart;

            if (cart == null)
                return View(new List<CartItem>());

            return View(cart.CartItems.ToList());
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveGuestCartItem(int prodId)
        {
            var guestCart = HttpContext.Session.GetString("GuestCart");
            if (!string.IsNullOrEmpty(guestCart))
            {
                var guestItems = System.Text.Json.JsonSerializer
                    .Deserialize<List<GuestCartItem>>(guestCart);

                guestItems = guestItems.Where(g => g.ProdId != prodId).ToList();

                HttpContext.Session.SetString("GuestCart",
                    System.Text.Json.JsonSerializer.Serialize(guestItems));
            }
            return RedirectToAction("ViewCart");
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

        //   [AuthorizeUser]
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

        //for the guest users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateGuestCartItem(int prodId, int quantity)
        {
            var guestCart = HttpContext.Session.GetString("GuestCart");
            if (!string.IsNullOrEmpty(guestCart))
            {
                var guestItems = System.Text.Json.JsonSerializer
                    .Deserialize<List<GuestCartItem>>(guestCart);

                var item = guestItems.FirstOrDefault(g => g.ProdId == prodId);
                if (item != null)
                {
                    if (quantity <= 0)
                        guestItems.Remove(item);
                    else
                        item.Quantity = quantity;
                }

                HttpContext.Session.SetString("GuestCart",
                    System.Text.Json.JsonSerializer.Serialize(guestItems));
            }
            return RedirectToAction("ViewCart");
        }

        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(string deliveryAddress)
        {

            // Redirect to login if not logged in
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                TempData["Error"] = "Please log in to complete your order.";
                return RedirectToAction("LogIn", "Authentication");
            }

            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;


            var cart = db.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.AddProduct)
                .FirstOrDefault(c => c.CustomerId == currentId && !c.IsCheckedOut);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("ViewCart");
            }

            var customer = db.Customers.FirstOrDefault(c => c.CustomerId == currentId);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("ViewCart");
            }

            if (!string.IsNullOrWhiteSpace(deliveryAddress))
            {
                db.Database.ExecuteSqlRaw(
                    "UPDATE Customers SET Address = {0} WHERE CustomerId = {1}",
                    deliveryAddress.Trim(), currentId);
            }

            var address = string.IsNullOrWhiteSpace(deliveryAddress)
                ? customer.Address
                : deliveryAddress.Trim();

            // Group cart items by store (OwnerId)
            var itemsByStore = cart.CartItems
                .Where(ci => ci.AddProduct != null)
                .GroupBy(ci => ci.AddProduct.OwnerId)
                .ToList();

            // Check stock for all items before creating any orders
            foreach (var storeGroup in itemsByStore)
            {
                foreach (var item in storeGroup)
                {
                    var product = db.AddProducts.Find(item.ProdId);
                    if (product == null) continue;
                    if (product.ProdQuantity < item.ProdQuantity)
                    {
                        TempData["Error"] = product.ProdName + " does not have enough stock available.";
                        return RedirectToAction("ViewCart");
                    }
                }
            }

            double deliveryFee = 50;
            var createdOrderIds = new List<int>();

            // Create one order per store
            foreach (var storeGroup in itemsByStore)
            {
                int storeOwnerId = storeGroup.Key;

                double storeSubtotal = storeGroup.Sum(ci =>
                    (ci.AddProduct != null ? ci.AddProduct.ProdPrice : ci.PriceAtTimeAdded) * ci.ProdQuantity);

                double storeTotalAmount = storeSubtotal + deliveryFee;

                //generating a random pin
                var rng = new Random();
                string deliveryPin = rng.Next(1000, 9999).ToString();

                var order = new Order
                {
                    CustomerId = currentId,
                    OwnerId = storeOwnerId,   // each order belongs to one store
                    OrderedOn = DateTime.Now,
                    Status = OrderStatus.Pending,
                    PaymentExpiry = DateTime.Now.AddMinutes(1),
                    TotalAmount = storeTotalAmount,
                    PaymentReference = "",
                    DeliveryAddress = address,
                    DeliveryPin = deliveryPin, //also added this
                    ScheduledDeliveryDate = DateTime.Now.AddMinutes(2), //i order li dilivwa after 2 days
                    OrderItems = storeGroup.Select(ci => new OrderItem
                    {
                        ProdId = ci.ProdId,
                        ProdName = ci.ProdName,
                        Quantity = ci.ProdQuantity,
                        PriceAtTimeOfPurchase = ci.PriceAtTimeAdded,
                        //---FOr refund columns
                        RefundOrExchange = GetRefund.No_Requests,
                        QuantityReturned = 0,
                        RetunReason = "",
                        ReturnPStatus = ReturnProduct.No_Request,
                        CollectionDate = null,
                        NewDeliveryDate = null
                    }).ToList()
                };

                db.Orders.Add(order);
                db.SaveChanges(); // save each order to get its OrderId

                createdOrderIds.Add(order.OrderId);

                // Deduct stock for this store's items
                foreach (var item in storeGroup)
                {
                    var product = db.AddProducts.Find(item.ProdId);
                    if (product == null) continue;
                    product.ProdQuantity -= item.ProdQuantity;
                    product.Status = product.ProdQuantity == 0 ? ProdStatus.Out_Of_Stock
                                   : product.ProdQuantity <= 5 ? ProdStatus.Low_stock
                                                                 : ProdStatus.In_Stock;
                }
            }

            // Mark cart as checked out
            cart.IsCheckedOut = true;
            db.SaveChanges();

            // Calculate grand total across all store orders
            double grandTotal = db.Orders
                .Where(o => createdOrderIds.Contains(o.OrderId))
                .Sum(o => o.TotalAmount);

            // Redirect to payment with all order IDs
            return RedirectToAction("PaymentPage", "Payment", new
            {
                orderIds = string.Join(",", createdOrderIds),
                amount = grandTotal,
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

        [AuthorizeUser]
        public IActionResult OrderHistory()
        {
            // The ! tells the compiler "trust me, this won't be null"
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;
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

        [AuthorizeUser]
        public IActionResult CustomerProfile()
        {
            // The ! tells the compiler "trust me, this won't be null"
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;
            DeleteExpiredOrders(currentId);
            return View(db.Customers.Where(c => c.CustomerId == currentId).ToList());
        }

        [AuthorizeUser]
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
                    .Include(o => o.ShopOwner) //Needed to show store name on the order
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
                    .Include(o => o.ShopOwner)
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

        [AuthorizeUser]
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


        [AuthorizeUser]
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

        public IActionResult SetReturnToCart()
        {
            TempData["ReturnToCart"] = true;
            return Ok();
        }

        //==========================================RETURN ITEM===========================
        public IActionResult ReturnPolicy() => View();  //view for return policy rules

        
        //displays all past orders and gives you permit to retun the item
        public IActionResult PastOrders()
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null) return RedirectToAction("LogIn", "Authentication");

            DeleteExpiredOrders(customerId.Value);

            var cutoff = DateTime.Now.AddDays(-7);

            // Flat list of OrderItems from eligible orders
            var items = db.OrderItems
                .Include(i => i.Order)
                .Include(i => i.Products)
                    .ThenInclude(p => p.ProductPictures)
                .Where(i => i.Order.CustomerId == customerId
                         && i.Order.DeliveredOn.HasValue
                         && i.RefundOrExchange == GetRefund.No_Requests  //if item money is refunded it must not appear. You can exchange as many times as you like
                         && i.ReturnPStatus == ReturnProduct.No_Request  //if item money is refunded it must not appear. You can exchange as many times as you like
                         && i.Order.DeliveredOn.Value >= cutoff)
                         //&& i.ReturnP != ReturnProduct.Item_Accepted) // hide already returned
                .OrderByDescending(i => i.Order.DeliveredOn)
                .ToList();

            return View(items);
        }

        //Submit the selected item for retun request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitItemReturn(int orderItemId, string returnReason,
                                       int quantityReturned, GetRefund refundOrExchange)
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null) return RedirectToAction("LogIn", "Authentication");

            var item = db.OrderItems
                .Include(i => i.Order)
                .FirstOrDefault(i => i.OrderItemId == orderItemId
                                  && i.Order.CustomerId == customerId);

            if (item == null) return NotFound();

            item.RetunReason = returnReason;
            item.QuantityReturned = quantityReturned;
            item.ReturnPStatus = ReturnProduct.Return_Pending;
            item.RefundOrExchange = refundOrExchange;   // ← from form

            db.SaveChanges();

            string choiceLabel = refundOrExchange == GetRefund.Refund ? "refund" : "exchange";
            TempData["Message"] = $"Return for '{item.ProdName}' submitted — {choiceLabel} requested.";
            return RedirectToAction("PastOrders");
        }


        //view all retuned products just like orders
        [AuthorizeUser]
        [HttpGet]
        public IActionResult MyReturns()
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null) return RedirectToAction("LogIn", "Authentication");

            var returnedItems = db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.DeliveryDriver)
                .Where(oi => oi.Order.CustomerId == customerId
                          && oi.ReturnPStatus != ReturnProduct.No_Request)
                .OrderByDescending(oi => oi.Order.OrderedOn)
                .ToList();

            return View(returnedItems);
        }


        //action to track the returned items
        [AuthorizeUser]
        [HttpGet]
        public IActionResult TrackReturn(int id)
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null) return RedirectToAction("LogIn", "Authentication");

            var item = db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.DeliveryDriver)
                .Include(oi => oi.Products)
                .FirstOrDefault(oi => oi.OrderItemId == id
                                   && oi.Order.CustomerId == customerId
                                   && oi.ReturnPStatus != ReturnProduct.No_Request);

            if (item == null)
            {
                ViewBag.Message = "Return not found or does not belong to you.";
                return View((CM_Project.Models.OrderItem)null);
            }

            return View(item);
        }


    }
}