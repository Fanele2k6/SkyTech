using SkyTech.Models;
using SkyTech.Models.Documents;
using SkyTech.Services;
using SkyTech.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SkyTech.Models.Repair_Item_Services;
using SkyTech.Models.Repair_Services;
using System;
using System.Collections.Generic;
using System.Linq;
using static SkyTech.Models.Order;

namespace SkyTech.Controllers
{
    // [AuthorizeUser]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext db;


        public CustomerController(ApplicationDbContext context, PayStackTransferService paystackService)
        {
            db = context;
            _paystackService = paystackService;
        }
        private readonly PayStackTransferService _paystackService;

        [HttpGet]
        public async Task<IActionResult> GetZaBanks()
        {
            var banks = await _paystackService.GetZaBanks();
            return Json(banks);
        }

        public IActionResult ProductDetails(int id)
        {
            var product = db.AddProducts
                .Include(p => p.ProductPictures)
                .Include(p => p.Owner)
                .FirstOrDefault(p => p.ProdId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpGet]
        public IActionResult ShoppingPage(string search, SelectCategory? category)
        {
            int? currentId = HttpContext.Session.GetInt32("UserId");

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
            var product = db.AddProducts.Find(id);
            if (product == null || product.Status == ProdStatus.Out_Of_Stock)
                return Content("No products / Out of stock");

            int? sessionId = HttpContext.Session.GetInt32("UserId");

            if (sessionId == null)
            {
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

                var prodIds = guestItems.Select(g => g.ProdId).ToList();
                var guestPictures = db.AddProducts
                    .Include(p => p.ProductPictures)
                    .Where(p => prodIds.Contains(p.ProdId))
                    .ToDictionary(p => p.ProdId, p => p.ProductPictures?.FirstOrDefault()?.FilePath);

                ViewBag.GuestPictures = guestPictures;
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
            ViewBag.AvailableBranches = db.shopowners
                                    .Where(s => s.ApplicationStatus == ApplicationStat.Approved)
                                    .ToList();

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


        //----------------GET Checkout -----------
        [AuthorizeUser]
        [HttpGet]
        public IActionResult Checkout()
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var cart = db.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.AddProduct)
                .FirstOrDefault(c => c.CustomerId == currentId && !c.IsCheckedOut);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("ViewCart");

            double subtotal = cart.CartItems.Sum(ci => ci.PriceAtTimeAdded * ci.ProdQuantity);

            var viewModel = new CheckoutViewModel
            {
                AvailableBranches = db.shopowners
                    .Where(s => s.ApplicationStatus == ApplicationStat.Approved)
                    .ToList(),
                CartSubtotal = subtotal,
                DeliveryFee = 50,
                PickupFee = 25
            };

            return View(viewModel);
        }


        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(string deliveryAddress, string orderType, int? selectedPickupBranchId, double Amount, string ProductName)
        {
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

            // ========== Handle address / order type ==========
            string address = customer.Address;

            if (orderType == "delivery")
            {
                if (!string.IsNullOrWhiteSpace(deliveryAddress))
                {
                    db.Database.ExecuteSqlRaw(
                        "UPDATE Customers SET Address = {0} WHERE CustomerId = {1}",
                        deliveryAddress.Trim(), currentId);
                    address = deliveryAddress.Trim();
                }
                else if (string.IsNullOrEmpty(customer.Address))
                {
                    TempData["Error"] = "Please provide a delivery address.";
                    return RedirectToAction("ViewCart");
                }
            }
            else if (orderType == "pickup")
            {
                if (!selectedPickupBranchId.HasValue)
                {
                    TempData["Error"] = "Please select a pickup branch.";
                    return RedirectToAction("ViewCart");
                }

                var branch = db.shopowners.Find(selectedPickupBranchId.Value);
                if (branch == null)
                {
                    TempData["Error"] = "Selected branch does not exist.";
                    return RedirectToAction("ViewCart");
                }
                address = $"Pickup at: {branch.ShopAddress}";
            }
            else
            {
                // Default to delivery for backward compatibility
                orderType = "delivery";
                if (!string.IsNullOrWhiteSpace(deliveryAddress))
                {
                    db.Database.ExecuteSqlRaw(
                        "UPDATE Customers SET Address = {0} WHERE CustomerId = {1}",
                        deliveryAddress.Trim(), currentId);
                    address = deliveryAddress.Trim();
                }
                else if (string.IsNullOrEmpty(customer.Address))
                {
                    TempData["Error"] = "Please provide a delivery address.";
                    return RedirectToAction("ViewCart");
                }
            }

            // ========== Group cart items by store ==========
            var itemsByStore = cart.CartItems
                .Where(ci => ci.AddProduct != null)
                .GroupBy(ci => ci.AddProduct.OwnerId)
                .ToList();

            // ========== Stock check before creating any orders ==========
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
            double pickupFee = 25;
            var createdOrderIds = new List<int>();
            PickupOrder createdPickupOrder = null;


            // ========== DELIVERY: One order per store ==========
            if (orderType == "delivery")
            {
                foreach (var storeGroup in itemsByStore)
                {
                    int storeOwnerId = storeGroup.Key;

                    double storeSubtotal = storeGroup.Sum(ci =>
                        (ci.AddProduct != null ? ci.AddProduct.ProdPrice : ci.PriceAtTimeAdded) * ci.ProdQuantity);

                    double storeTotalAmount = storeSubtotal + deliveryFee;

                    var rng = new Random();
                    string deliveryPin = rng.Next(1000, 9999).ToString();

                    var order = new Order
                    {
                        CustomerId = currentId,
                        OwnerId = storeOwnerId,
                        OrderedOn = DateTime.Now,
                        Status = OrderStatus.Pending,
                        PaymentExpiry = DateTime.Now.AddMinutes(1),
                        TotalAmount = storeTotalAmount,
                        PaymentReference = "",
                        DeliveryAddress = address,
                        DeliveryPin = deliveryPin,
                        ScheduledDeliveryDate = DateTime.Now.AddMinutes(2),
                        OrderType = DeliveryType.HomeDelivery,
                        OrderItems = storeGroup.Select(ci => new OrderItem
                        {
                            ProdId = ci.ProdId,
                            ProdName = ci.ProdName,
                            Quantity = ci.ProdQuantity,
                            PriceAtTimeOfPurchase = ci.PriceAtTimeAdded,
                            RefundOrExchange = GetRefund.No_Requests,
                            QuantityReturned = 0,
                            RetunReason = "",
                            ReturnPStatus = ReturnProduct.No_Request,
                            CollectionDate = null,
                            NewDeliveryDate = null
                        }).ToList()
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();
                    createdOrderIds.Add(order.OrderId);

                    // Deduct stock
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
            }


            // ========== PICKUP: ONE order containing ALL items ==========
            else
            {
                double totalSubtotal = itemsByStore.Sum(storeGroup =>
                    storeGroup.Sum(ci =>
                        (ci.AddProduct != null ? ci.AddProduct.ProdPrice : ci.PriceAtTimeAdded) * ci.ProdQuantity));

                double totalAmount = totalSubtotal + pickupFee;

                var rng = new Random();
                string deliveryPin = rng.Next(1000, 9999).ToString();

                // Create the order with an empty items list — items added in the loop below
                var order = new Order
                {
                    CustomerId = currentId,
                    OwnerId = selectedPickupBranchId.Value,
                    OrderedOn = DateTime.Now,
                    Status = OrderStatus.Pending,
                    PaymentExpiry = DateTime.Now.AddMinutes(1),
                    TotalAmount = totalAmount,
                    PaymentReference = "",
                    DeliveryAddress = address,
                    DeliveryPin = deliveryPin,
                    ScheduledDeliveryDate = DateTime.Now.AddMinutes(2),
                    OrderType = DeliveryType.InStorePickup,
                    OrderItems = new List<OrderItem>()
                };

                // Add ALL items from ALL stores into the single order
                foreach (var storeGroup in itemsByStore)
                {
                    foreach (var cartItem in storeGroup)
                    {
                        order.OrderItems.Add(new OrderItem
                        {
                            ProdId = cartItem.ProdId,
                            ProdName = cartItem.ProdName,
                            Quantity = cartItem.ProdQuantity,
                            PriceAtTimeOfPurchase = cartItem.PriceAtTimeAdded,
                            RefundOrExchange = GetRefund.No_Requests,
                            QuantityReturned = 0,
                            RetunReason = "",
                            ReturnPStatus = ReturnProduct.No_Request,
                            CollectionDate = null,
                            NewDeliveryDate = null
                        });
                    }
                }

                db.Orders.Add(order);
                db.SaveChanges();
                createdOrderIds.Add(order.OrderId);

                // Create pickup order record
                string pickupReference = GeneratePickupReference();
                createdPickupOrder = new PickupOrder
                {
                    OrderId = order.OrderId,
                    PickupBranchId = selectedPickupBranchId.Value,
                    PickupReference = pickupReference,
                    PickupFee = pickupFee,
                    Status = PickupStatus.Pending,
                    ScheduledPickupDate = DateTime.Now.AddDays(2)
                };
                db.PickupOrders.Add(createdPickupOrder);
                db.SaveChanges();

                order.PickupOrderId = createdPickupOrder.PickupOrderId;
                db.SaveChanges();

                // Deduct stock and create inter-branch transfers where needed
                foreach (var storeGroup in itemsByStore)
                {
                    int sourceBranchId = storeGroup.Key;

                    // Deduct stock
                    foreach (var item in storeGroup)
                    {
                        var product = db.AddProducts.Find(item.ProdId);
                        if (product == null) continue;
                        product.ProdQuantity -= item.ProdQuantity;
                        product.Status = product.ProdQuantity == 0 ? ProdStatus.Out_Of_Stock
                                       : product.ProdQuantity <= 5 ? ProdStatus.Low_stock
                                                                    : ProdStatus.In_Stock;
                    }

                    // If items are NOT from the pickup branch, create a transfer record
                    if (sourceBranchId != selectedPickupBranchId.Value)
                    {
                        foreach (var cartItem in storeGroup)
                        {
                            var orderItem = order.OrderItems
                                .FirstOrDefault(oi => oi.ProdId == cartItem.ProdId);

                            if (orderItem != null)
                            {
                                var transfer = new InterBranchTransfer
                                {
                                    OrderItemId = orderItem.OrderItemId,
                                    SourceBranchId = sourceBranchId,
                                    TargetBranchId = selectedPickupBranchId.Value,
                                    TransferCode = GenerateTransferCode(),
                                    TransferStatus = ItemTransferStatus.PendingAtSource,
                                    ScheduledTransferDate = DateTime.Now.AddDays(1),
                                    IsDelivered = false
                                };
                                db.InterBranchTransfers.Add(transfer);
                            }
                        }
                    }
                }
                db.SaveChanges();
            }

            // Mark cart as checked out
            cart.IsCheckedOut = true;
            db.SaveChanges();

            // Grand total across all created orders
            double grandTotal = db.Orders
                .Where(o => createdOrderIds.Contains(o.OrderId))
                .Sum(o => o.TotalAmount);

            string orderIdsString = string.Join(",", createdOrderIds);

            return RedirectToAction("PaymentPage", "Payment", new
            {
                orderIds = orderIdsString,
                amount = grandTotal,
                customerEmail = customer.Email,
                customerName = customer.FirstName,
                itemCount = cart.CartItems.Sum(i => i.ProdQuantity),
                deliveryAddress = address,
                orderType = orderType,
                pickupReference = orderType == "pickup" && createdPickupOrder != null
                    ? createdPickupOrder.PickupReference : ""
            });
        }


        private string GeneratePickupReference()
        {
            var rng = new Random();
            string prefix = "PICK";
            string numbers = rng.Next(10000, 99999).ToString();
            string suffix = DateTime.Now.ToString("MMdd");
            return $"{prefix}-{numbers}-{suffix}";
        }

        private string GenerateTransferCode()
        {
            var rng = new Random();
            string prefix = "TRANS";
            string numbers = rng.Next(100000, 999999).ToString();
            return $"{prefix}-{numbers}";
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
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;
            DeleteExpiredOrders(currentId);
            return View(db.Customers.Where(c => c.CustomerId == currentId).ToList());
        }

        [AuthorizeUser]
        public IActionResult CustomerDash()
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;
            DeleteExpiredOrders(currentId);

            // Get active orders count
            var activeOrders = db.Orders
                .Where(o => o.CustomerId == currentId && !o.IsPaid)
                .Count();

            // Get repair requests stats
            var pendingRepairs = db.RepairRequests
                .Where(r => r.CustomerId == currentId && r.Status == SkyTech.Models.Repair_Item_Services.RepairStatus.Pending)
                .Count();

            var inProgressRepairs = db.RepairRequests
                .Where(r => r.CustomerId == currentId && r.Status == SkyTech.Models.Repair_Item_Services.RepairStatus.InProgress)
                .Count();

            var completedRepairs = db.RepairRequests
                .Where(r => r.CustomerId == currentId && r.Status == SkyTech.Models.Repair_Item_Services.RepairStatus.Completed)
                .Count();

            ViewBag.ActiveOrders = activeOrders;
            ViewBag.PendingRepairs = pendingRepairs + inProgressRepairs;
            ViewBag.CompletedRepairs = completedRepairs;

            return View(db.Customers.Where(c => c.CustomerId == currentId).ToList());
        }

        [AuthorizeUser]
        public IActionResult CustomerRepairRequests()
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var requests = db.RepairRequests
                .Where(r => r.CustomerId == currentId)
                .Include(r => r.Category)
                .OrderByDescending(r => r.DateCreated)
                .ToList();

            return View(requests);
        }


        [AuthorizeUser]
        [HttpGet]
        public IActionResult TrackOrder(int id)
        {
            var order = db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Driver)
                .Include(o => o.ShopOwner)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                ViewBag.Message = "Order not found.";
                return View();
            }

            if (order.OrderType == DeliveryType.InStorePickup)
            {
                var pickupOrder = db.PickupOrders
                    .Include(p => p.PickupBranch)
                    .FirstOrDefault(p => p.OrderId == id);
                ViewBag.PickupOrder = pickupOrder;

                var transfers = db.InterBranchTransfers
                    .Include(t => t.SourceBranch)
                    .Where(t => t.OrderItem.OrderId == id)
                    .ToList();
                ViewBag.TransfersForOrder = transfers;
            }

            string statusMessage = order.Status switch
            {
                OrderStatus.Pending => "Order confirmed",
                OrderStatus.Assigned => "Driver assigned",
                OrderStatus.OutForDelivery => "Out for delivery",
                OrderStatus.Delivered => "Delivered",
                _ => "Processing"
            };
            ViewBag.StatusMessage = statusMessage;

            return View(order);
        }


        private string GetStatusMessage(SkyTech.Models.OrderStatus status) =>
            status == SkyTech.Models.OrderStatus.Pending ? "Pending"
          : status == SkyTech.Models.OrderStatus.Assigned ? "Driver Assigned"
          : status == SkyTech.Models.OrderStatus.OutForDelivery ? "Out For Delivery"
          : status == SkyTech.Models.OrderStatus.Delivered ? "Delivered"
          : "";


        // ── REPAIR REQUEST TRACKING ────────────────────────────────

        [AuthorizeUser]
        [HttpGet]
        public IActionResult MyRepairRequests()
        {
            var customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null)
                return RedirectToAction("LogIn", "Authentication");

            var repairs = db.RepairRequests
                .Where(r => r.CustomerId == customerId)
                .Include(r => r.Category)
                .OrderByDescending(r => r.DateCreated)
                .ToList();

            return View(repairs);
        }

        [AuthorizeUser]
        [HttpGet]
        public IActionResult ViewRepairDetail(int? id)
        {
            if (id == null)
                return NotFound();

            var customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null)
                return RedirectToAction("LogIn", "Authentication");

            var repair = db.RepairRequests
                .Where(r => r.RequestId == id && r.CustomerId == customerId)
                .Include(r => r.Category)
                .FirstOrDefault();

            if (repair == null)
                return NotFound();

            var assignment = db.RepairAssignments
                .Include(a => a.Technician)
                .FirstOrDefault(a => a.RepairRequestId == id);

            ViewBag.Assignment = assignment;

            var quotations = db.Quotations
                .Where(q => q.RepairRequestId == id)
                .Include(q => q.QuotationItems)
                .Include(q => q.Technician)
                .ToList();

            ViewBag.Quotations = quotations;

            var trackingHistory = db.RepairTrackings
                .Where(t => t.RepairRequestId == id)
                .OrderByDescending(t => t.UpdatedDate)
                .ToList();

            ViewBag.TrackingHistory = trackingHistory;

            return View(repair);
        }

        [AuthorizeUser]
        [HttpPost]
        public IActionResult ApproveQuotation(int quotationId, int repairId)
        {
            var customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null)
                return RedirectToAction("LogIn", "Authentication");

            var quotation = db.Quotations.Find(quotationId);
            if (quotation == null)
                return NotFound();

            var repair = db.RepairRequests
                .FirstOrDefault(r => r.RequestId == repairId && r.CustomerId == customerId);
            if (repair == null)
                return NotFound();

            quotation.Status = QuotationStatus.Approved;
            repair.Status = RepairStatus.InProgress;

            var tracking = new RepairTracking
            {
                RepairRequestId = repairId,
                Status = RepairStatus.InProgress,
                UpdatedDate = DateTime.Now,
                StatusMessage = "Customer approved quotation. Repair work will begin shortly."
            };

            db.RepairTrackings.Add(tracking);
            db.SaveChanges();

            return RedirectToAction("ViewRepairDetail", new { id = repairId });
        }

        [AuthorizeUser]
        [HttpPost]
        public IActionResult RejectQuotation(int quotationId, int repairId)
        {
            var customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null)
                return RedirectToAction("LogIn", "Authentication");

            var quotation = db.Quotations.Find(quotationId);
            if (quotation == null)
                return NotFound();

            var repair = db.RepairRequests
                .FirstOrDefault(r => r.RequestId == repairId && r.CustomerId == customerId);
            if (repair == null)
                return NotFound();

            quotation.Status = QuotationStatus.Rejected;
            repair.Status = RepairStatus.Pending;

            var tracking = new RepairTracking
            {
                RepairRequestId = repairId,
                Status = RepairStatus.Pending,
                UpdatedDate = DateTime.Now,
                StatusMessage = "Customer rejected quotation. Please submit a revised quotation."
            };

            db.RepairTrackings.Add(tracking);
            db.SaveChanges();

            return RedirectToAction("ViewRepairDetail", new { id = repairId });
        }

        [HttpGet]
        public IActionResult GetOrderStatus(int orderId)
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            int step = order.Status == SkyTech.Models.OrderStatus.Pending ? 0
                     : order.Status == SkyTech.Models.OrderStatus.Assigned ? 1
                     : order.Status == SkyTech.Models.OrderStatus.OutForDelivery ? 2
                     : order.Status == SkyTech.Models.OrderStatus.Delivered ? 3 : 0;

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

            if (order.Status != SkyTech.Models.OrderStatus.Delivered)
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
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;

            var orders = db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Driver)
                .Where(o => o.CustomerId == currentId)
                .OrderByDescending(o => o.OrderedOn)
                .ToList();

            var pickupOrders = db.PickupOrders
                .Include(p => p.PickupBranch)
                .Where(p => orders.Select(o => o.OrderId).Contains(p.OrderId))
                .ToList();
            ViewBag.PickupOrders = pickupOrders;

            return View(orders);
        }


        public IActionResult SetReturnToCart()
        {
            TempData["ReturnToCart"] = true;
            return Ok();
        }


        //==========================================RETURN / REFUND===========================

        public IActionResult ReturnPolicy() => View();

        public IActionResult PastOrders()
        {
            int? customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null) return RedirectToAction("LogIn", "Authentication");

            DeleteExpiredOrders(customerId.Value);

            var cutoff = DateTime.Now.AddDays(-7);

            var items = db.OrderItems
                .Include(i => i.Order)
                .Include(i => i.Products)
                    .ThenInclude(p => p.ProductPictures)
                .Where(i => i.Order.CustomerId == customerId
                         && i.Order.DeliveredOn.HasValue
                         && i.RefundOrExchange == GetRefund.No_Requests
                         && i.ReturnPStatus == ReturnProduct.No_Request
                         && i.Order.DeliveredOn.Value >= cutoff)
                .OrderByDescending(i => i.Order.DeliveredOn)
                .ToList();

            return View(items);
        }


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
            item.RefundOrExchange = refundOrExchange;

            db.SaveChanges();

            string choiceLabel = refundOrExchange == GetRefund.Refund ? "refund" : "exchange";
            TempData["Message"] = $"Return for '{item.ProdName}' submitted — {choiceLabel} requested.";
            return RedirectToAction("PastOrders");
        }


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
                return View((SkyTech.Models.OrderItem)null);
            }

            return View(item);
        }


        //=====================================Increament 2===============================
        [AuthorizeUser]
        public IActionResult ProposeItem() => View();

        [HttpPost]
        [AuthorizeUser]
        public async Task<IActionResult> ProposeItem(ProposeItemViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            int currentId = HttpContext.Session.GetInt32("UserId").Value;

            var address = db.Customers
                .Where(c => c.CustomerId == currentId)
                .Select(c => c.Address)
                .FirstOrDefault();

            var model = new ProposeItem
            {
                CustomerId = currentId,  // ← this line was missing
                ProdName = vm.ProdName,
                ProdCategory = vm.ProdCategory,
                StoreAddress = "",
                CustomerAddress = address,
                ProdCondition = vm.ProdCondition,
                ProdDescription = vm.ProdDescription,
                ProdStatus = Status.Pending,
                SavedOn = DateTime.Now
            };

            // ================= RECEIPT IMAGES =================
            if (vm.ReceiptPic != null && vm.ReceiptPic.Any())
            {
                string receiptFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/receipts");

                if (!Directory.Exists(receiptFolder))
                    Directory.CreateDirectory(receiptFolder);

                foreach (var file in vm.ReceiptPic)
                {
                    if (file.Length > 0)
                    {
                        string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        string fullPath = Path.Combine(receiptFolder, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        model.ReceiptPics.Add(new ReceiptPic
                        {
                            CustomerId = currentId,
                            FileName = fileName,
                            FilePath = "/uploads/receipts/" + fileName, // ✅ FIXED
                            FileType = file.ContentType,
                            SavedOn = DateTime.Now
                        });
                    }
                }
            }

            // ================= PRODUCT IMAGES =================
            if (vm.ProposedProdPic != null && vm.ProposedProdPic.Any())
            {
                string productFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/products");

                if (!Directory.Exists(productFolder))
                    Directory.CreateDirectory(productFolder);

                foreach (var file in vm.ProposedProdPic)
                {
                    if (file.Length > 0)
                    {
                        string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        string fullPath = Path.Combine(productFolder, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        model.ProposedProdPics.Add(new ProposedProdPic
                        {
                            CustomerId = currentId,
                            FileName = fileName,
                            FilePath = "/uploads/products/" + fileName, // ✅ FIXED
                            FileType = file.ContentType,
                            SavedOn = DateTime.Now
                        });
                    }
                }
            }

            db.ProposeItems.Add(model);
            await db.SaveChangesAsync();

            return RedirectToAction("ProposeItem");
        }

        [AuthorizeUser]
        public IActionResult MyProposals()
        {
            int currentId = HttpContext.Session.GetInt32("UserId").Value;

            var proposals = db.ProposeItems
                .Include(p => p.ReceiptPics)
                .Include(p => p.ProposedProdPics)
                .Where(p => p.CustomerId == currentId && p.ProdStatus == Status.Pending || p.ProdStatus == Status.Approved)
                .OrderByDescending(p => p.PropItemId)
                .ToList();

            return View(proposals);
        }
        // Customer accepts the shop owner's proposed amount
        [HttpPost]
        [AuthorizeUser]
        public IActionResult CustomerAcceptOffer(int id)
        {
            var proposal = db.ProposeItems.Find(id);
            if (proposal == null) return NotFound();

            proposal.ProdStatus = Status.CustomerAccepted;
            db.SaveChanges();

            // ← id was not being passed, so ProposalCheckout received id = 0
            return RedirectToAction("ProposalCheckout", new { id = id });
        }

        // Customer declines the shop owner's proposed amount
        [HttpPost]
        [AuthorizeUser]
        public IActionResult CustomerDeclineOffer(int id)
        {
            var proposal = db.ProposeItems.Find(id);
            if (proposal == null) return NotFound();

            proposal.ProdStatus = Status.CustomerDeclined;
            db.SaveChanges();

            return RedirectToAction("MyProposals");
        }

        // Customer cancels/withdraws their proposal entirely
        [HttpPost]
        [AuthorizeUser]
        public IActionResult CancelProposal(int id)
        {
            var proposal = db.ProposeItems
                .Include(p => p.ReceiptPics)
                .Include(p => p.ProposedProdPics)
                .FirstOrDefault(p => p.PropItemId == id);

            if (proposal == null) return NotFound();

            db.ProposeItems.Remove(proposal);
            db.SaveChanges();

            return RedirectToAction("MyProposals");
        }

        [AuthorizeUser]
        [HttpGet]
        public IActionResult ProposalCheckout(int id)
        {
            var proposal = db.ProposeItems.Find(id);
            if (proposal == null) return NotFound();

            var vm = new ProposalCheckoutViewModel
            {
                PropItemId = proposal.PropItemId,
                ProdName = proposal.ProdName,
                AmountProposed = proposal.AmountProposed

            };
            return View(vm);
        }

        private int GetCustomerId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        private bool IsCustomer()
        {
            var userType = HttpContext.Session.GetString("UserType");
            return userType == "Customer";
        }

        [HttpGet]
        public async Task<IActionResult> ViewQuotation(int id)
        {
            if (!IsCustomer()) return RedirectToAction("LogIn", "Authentication");

            var customerId = GetCustomerId();

            var quotation = await db.Quotations
                .Include(q => q.QuotationItems)
                .Include(q => q.Technician)
                .Include(q => q.RepairRequest)
                .FirstOrDefaultAsync(q => q.QuotationId == id &&
                                          q.RepairRequest.CustomerId == customerId);

            if (quotation == null) return NotFound();

            var vm = new QuotationResponseViewModel
            {
                QuotationId = quotation.QuotationId,
                RepairRequestId = quotation.RepairRequestId,
                ItemName = quotation.RepairRequest.ItemName,
                IssueDescription = quotation.RepairRequest.IssueDescription,
                TechnicianName = quotation.Technician?.FullName,
                TotalCost = quotation.TotalCost,
                TechnicianNotes = quotation.TechnicianNotes,
                CreatedDate = quotation.CreatedDate,
                QuotationItems = quotation.QuotationItems.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [AuthorizeUser]
        public IActionResult ProposalCheckout(ProposalCheckoutViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            int currentId = HttpContext.Session.GetInt32("UserId").Value;

            var checkout = new ProposalCheckout
            {
                PropItemId = vm.PropItemId,
                CustomerId = currentId,
                SelectDelivery = vm.SelectDelivery,
                PhoneNumber = vm.PhoneNumber,
                BankCode = vm.BankCode,
                Address = vm.Address,
                BankName = vm.BankName,
                AccountHolderName = vm.AccountHolderName,
                AccountNumber = vm.AccountNumber,
                BranchCode = vm.BranchCode,
                AccountType = vm.AccountType,
                SubmittedOn = DateTime.Now
            };

            db.ProposalCheckouts.Add(checkout);

            // ── Update status on the ProposeItem ──────────────────────
            var propItem = db.ProposeItems
                             .FirstOrDefault(p => p.PropItemId == vm.PropItemId);

            if (propItem != null)
                propItem.ProdStatus = Status.CustomerAccepted;
            // ─────────────────────────────────────────────────────────

            db.SaveChanges();

            return RedirectToAction("MyProposals");
        }

        //db.ProposalCheckouts.Add(checkout);
        //    db.SaveChanges();

        //    return RedirectToAction("MyProposals");
        //}

        // Also update the redirect inside CustomerAcceptOffer
        [HttpPost]
        [AuthorizeUser]
        public IActionResult AcceptProposalOffer(int id)
        {
            var proposal = db.ProposeItems.Find(id);
            if (proposal == null) return NotFound();

            proposal.ProdStatus = Status.CustomerAccepted;
            db.SaveChanges();

            return RedirectToAction("ProposalCheckout", new { id = id }); // ← updated
        }




        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptQuotation(int quotationId)
        {
            var quotation = await db.Quotations
                .Include(q => q.RepairRequest)
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId);

            if (quotation == null) return NotFound();

            quotation.Status = QuotationStatus.Approved;
            quotation.RepairRequest.Status = RepairStatus.InProgress;

            db.RepairTrackings.Add(new RepairTracking
            {
                RepairRequestId = quotation.RepairRequestId,
                Status = RepairStatus.InProgress,
                UpdatedDate = DateTime.Now,
                StatusMessage = "Customer accepted the quotation. Repair is now active."
            });

            await db.SaveChangesAsync();
            TempData["Success"] = "Quotation accepted! Your repair is now active.";
            return RedirectToAction("MyRepairs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectQuotation(int quotationId, string rejectionReason)
        {
            var quotation = await db.Quotations
                .Include(q => q.RepairRequest)
                .Include(q => q.Technician)
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId);

            if (quotation == null) return NotFound();

            quotation.Status = QuotationStatus.Rejected;
            quotation.RepairRequest.Status = RepairStatus.Cancelled;

            if (quotation.Technician != null)
                quotation.Technician.Status = AvailabilityStatus.Available;

            db.RepairTrackings.Add(new RepairTracking
            {
                RepairRequestId = quotation.RepairRequestId,
                Status = RepairStatus.Cancelled,
                UpdatedDate = DateTime.Now,
                StatusMessage = $"Customer rejected the quotation. Reason: {rejectionReason ?? "None provided"}"
            });

            await db.SaveChangesAsync();

            TempData["Message"] = "Quotation rejected. The repair has been cancelled.";
            return RedirectToAction("MyRepairs");
        }

      

    }

    public class QuotationResponseViewModel
    {
        public int QuotationId { get; set; }
        public int RepairRequestId { get; set; }
        public string ItemName { get; set; }
        public string IssueDescription { get; set; }
        public string TechnicianName { get; set; }
        public decimal TotalCost { get; set; }
        public string TechnicianNotes { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<QuotationItem> QuotationItems { get; set; }
        public string RejectionReason { get; set; }
    }

   
}
  
            