using CM_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CM_Project.Services;

namespace CM_Project.Controllers
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
        public IActionResult PaymentPage(int orderId, decimal amount,
            string customerEmail, string customerName)
        {
            var model = new PaymentRequest
            {
                OrderId = orderId,
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

        public async Task<IActionResult> Verify(string reference)
        {
            var result = await _payStack.VerifyTransaction(reference);

            if (result != null && result.status && result.data.status == "success")
            {
                //var db = new ApplicationDbContext(); // ideally inject via DI

                var parts = reference.Split('-');
                int orderId = Convert.ToInt32(parts[1]);

                var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
                if (order != null)
                {
                    order.IsPaid = true;
                    order.PaymentReference = reference;
                    order.Status = OrderStatus.Pending;
                }

                var cart = db.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefault(c => c.CustomerId == order.CustomerId && !c.IsCheckedOut);

                if (cart != null)
                {
                    db.CartItems.RemoveRange(cart.CartItems);
                    cart.IsCheckedOut = true;
                }

                ViewBag.Reference = reference;
                ViewBag.Amount = result.data.amount / 100m;
                ViewBag.Email = result.data.customer.email;

                db.SaveChanges();
                return View("Success");
            }

            return View("Failed");
        }

        public IActionResult Failed() => View();
    }
}
