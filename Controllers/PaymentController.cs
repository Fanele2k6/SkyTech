using SkyTech.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using SkyTech.Services;

namespace SkyTech.Controllers
{
    [AuthorizeUser]
    public class PaymentController : Controller
    {
        //private readonly PayStackService _payStack = new PayStackService();
        //private readonly ApplicationDbContext db;  // ← add this

        //public PaymentController(ApplicationDbContext context)  // ← inject it
        //{
        //    db = context;
        //    _payStack = new PayStackService();
        //}

        private readonly PayStackService _payStack;
        private readonly ApplicationDbContext db;

        public PaymentController(ApplicationDbContext context, PayStackService payStack)
        {
            db = context;
            _payStack = payStack; // ← injected, no more new PayStackService()
        }
        public IActionResult PaymentPage(string orderIds, decimal amount,
            string customerEmail, string customerName)
        {
            var model = new PaymentRequest
            {
                OrderIds = orderIds,
                Amount = amount,
                CustomerEmail = customerEmail,
                CustomerName = customerName
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(PaymentRequest model)
        {
            string reference = $"ORDER-{model.OrderId}-{DateTime.Now.Ticks}";
            string callbackUrl = Url.Action("Verify", "Payment",
                new { reference }, Request.Scheme);

            var initResult = await _payStack.InitializeTransaction(
                model.CustomerEmail, model.Amount, reference, callbackUrl);

            if (initResult != null && initResult.status)
            {
                HttpContext.Session.SetString("paystack_reference", initResult.data.reference);
                HttpContext.Session.SetString("order_amount", model.Amount.ToString());
                HttpContext.Session.SetInt32("order_id", model.OrderId);

                ViewBag.AccessCode = initResult.data.access_code;
                ViewBag.PublicKey = "pk_test_00e0ace8896dc183d605bed4ae78c29197fa3029";
                ViewBag.Email = model.CustomerEmail;
                ViewBag.AmountKobo = (int)(model.Amount * 100);
                ViewBag.CustomerName = model.CustomerName;
                ViewBag.Reference = initResult.data.reference;
                ViewBag.CallbackUrl = callbackUrl;

                return View("PaymentPage");
            }

            TempData["Error"] = "Could not initialize payment. Please try again.";
            return RedirectToAction("Checkout");
        }


        public async Task<IActionResult> Verify(string reference, string orderIds)
{
    var result = await _payStack.VerifyTransaction(reference);
    if (result != null && result.status && result.data.status == "success")
    {
        if (string.IsNullOrWhiteSpace(orderIds))
            return View("Failed");

        // Parse all order IDs
        var ids = orderIds.Split(',')
            .Select(id => int.TryParse(id.Trim(), out int x) ? x : 0)
            .Where(id => id > 0)
            .ToList();

        if (!ids.Any())
            return View("Failed");

        // Mark all orders as paid
        var orders = db.Orders.Where(o => ids.Contains(o.OrderId)).ToList();
        if (!orders.Any())
            return View("Failed");

        foreach (var order in orders)
        {
            order.IsPaid           = true;
            order.PaymentReference = reference;
            order.Status           = OrderStatus.Pending;
        }

                // Clear the cart using CustomerId from first order
                int customerId = orders.First().CustomerId ?? 0;
                var cart = db.Carts
            .Include(c => c.CartItems)
            .FirstOrDefault(c => c.CustomerId == customerId && !c.IsCheckedOut);

        if (cart != null)
        {
            db.CartItems.RemoveRange(cart.CartItems);
            cart.IsCheckedOut = true;
        }

        db.SaveChanges();

        ViewBag.Reference = reference;
        ViewBag.Amount    = result.data.amount / 100m;
        ViewBag.Email     = result.data.customer.email;

        return View("Success");
    }

    return View("Failed");
}



        //public async Task<IActionResult> Verify(string reference)
        //{
        //    var result = await _payStack.VerifyTransaction(reference);
        //    if (result != null && result.status && result.data.status == "success")
        //    {
        //        // reference format is ORDER-{orderId}-{timestamp}
        //        var parts = reference.Split('-');
        //        if (parts.Length < 2)
        //            return View("Failed");

        //        if (!int.TryParse(parts[1], out int orderId))
        //            return View("Failed");

        //        var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);

        //        // Guard — if order not found don't crash
        //        if (order == null)
        //            return View("Failed");

        //        order.IsPaid = true;
        //        order.PaymentReference = reference;
        //        order.Status = OrderStatus.Pending;

        //        // Now safe to use order.CustomerId
        //        var cart = db.Carts
        //            .Include(c => c.CartItems)
        //            .FirstOrDefault(c => c.CustomerId == order.CustomerId && !c.IsCheckedOut);

        //        if (cart != null)
        //        {
        //            db.CartItems.RemoveRange(cart.CartItems);
        //            cart.IsCheckedOut = true;
        //        }

        //        db.SaveChanges();

        //        ViewBag.Reference = reference;
        //        ViewBag.Amount = result.data.amount / 100m;
        //        ViewBag.Email = result.data.customer.email;

        //        return View("Success");
        //    }

        //    return View("Failed");
        //}



        //public async Task<IActionResult> Verify(string reference)
        //{
        //    var result = await _payStack.VerifyTransaction(reference);

        //    if (result != null && result.status && result.data.status == "success")
        //    {
        //        //var db = new ApplicationDbContext(); // ideally inject via DI

        //        var parts = reference.Split('-');
        //        int orderId = Convert.ToInt32(parts[1]);

        //        var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        //        if (order != null)
        //        {
        //            order.IsPaid = true;
        //            order.PaymentReference = reference;
        //            order.Status = OrderStatus.Pending;
        //        }

        //        var cart = db.Carts
        //            .Include(c => c.CartItems)
        //            .FirstOrDefault(c => c.CustomerId == order.CustomerId && !c.IsCheckedOut);

        //        if (cart != null)
        //        {
        //            db.CartItems.RemoveRange(cart.CartItems);
        //            cart.IsCheckedOut = true;
        //        }

        //        ViewBag.Reference = reference;
        //        ViewBag.Amount = result.data.amount / 100m;
        //        ViewBag.Email = result.data.customer.email;

        //        db.SaveChanges();
        //        return View("Success");
        //    }

        //    return View("Failed");
        //}

        public IActionResult Failed() => View();
    }
}
