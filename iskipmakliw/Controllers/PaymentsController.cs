using iskipmakliw.Data;
using iskipmakliw.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;

namespace iskipmakliw.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymongo _paymongo;
        ApplicationDbContext _context;

        public PaymentsController(IPaymongo paymongo, ApplicationDbContext context)
        {
            _paymongo = paymongo;
            _context = context;
        }

        [HttpGet]
        public IActionResult Checkout(int Id)
        {
            int? userId = HttpContext.Session.GetInt32("UsersId");
            if (userId == null)
                return RedirectToAction("Logout", "Account");

            var payments = _context.Payments
                            .Include(p => p.Users)
                            .ThenInclude(u => u.UserDetails)
                            .ThenInclude(u => u.Plans)
                            .Where(p => p.UsersId == userId && p.Status == "Pending" && p.Id == Id && p.DueDate >= DateTime.Now)
                            .ToList();
            if(payments == null)
            {
                return RedirectToAction("Index", "Seller");
            }
            return View(payments);
        }

        [HttpPost]
        public async Task<IActionResult> PayNow(decimal totalAmount, string productNames, string paymentIds)
        {
            var name = HttpContext.Session.GetString("Username") ?? "Guest";
            var email = HttpContext.Session.GetString("Email") ?? "guest@example.com";
            var contact = HttpContext.Session.GetString("ContactNumber") ?? "0000000000";

            var sessionJson = await _paymongo.CreateCheckoutSession(
                totalAmount,
                "PHP",
                name,
                email,
                contact,
                productNames
            );

            dynamic session = JsonConvert.DeserializeObject(sessionJson);
            string checkoutUrl = session?.data?.attributes?.checkout_url;
            string sessionId = session?.data?.id;

            if (string.IsNullOrEmpty(checkoutUrl) || string.IsNullOrEmpty(sessionId))
            {
                TempData["Error"] = "Unable to create payment session.";
                return RedirectToAction("Checkout");
            }

            // save for later validation
            TempData["PaymentIds"] = paymentIds;
            TempData["SessionId"] = sessionId;

            return Redirect(checkoutUrl);
        }


        public async Task<IActionResult> Success()
        {
            var sessionId = TempData["SessionId"]?.ToString();
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session not found.");
            }

            // Ask PayMongo about this checkout session
            var sessionJson = await _paymongo.GetCheckoutSession(sessionId);
            dynamic session = JsonConvert.DeserializeObject(sessionJson);

            // payments is an array; get first payment
            var payments = session?.data?.attributes?.payments as IEnumerable<dynamic>;
            var first = payments?.FirstOrDefault();
            string status = first?.attributes?.status;

            // Update DB
            var paymentIds = TempData["PaymentIds"]?.ToString()?.Split(',').Select(int.Parse).ToList();

            if (paymentIds != null && paymentIds.Any())
            {
                // Update payment records
                var paymentsData = _context.Payments.Where(p => paymentIds.Contains(p.Id)).ToList();
                foreach (var payment in paymentsData)
                {
                    payment.Status = status switch
                    {
                        "paid" => "Paid",
                        "succeeded" => "Paid",
                        "failed" => "Failed",
                        _ => "Pending"
                    };
                }

                // Get user details of the first payment
                var userId = paymentsData.FirstOrDefault()?.UsersId;
                if (userId != null)
                {
                    var userDetails = await _context.UserDetails.FirstOrDefaultAsync(u => u.UsersId == userId);
                    if (userDetails != null)
                    {
                        userDetails.Status = "For approval";
                    }
                }

                await _context.SaveChangesAsync();
            }

            ViewBag.PaymentStatus = status ?? "unknown";
            return View();
        }



        public IActionResult Cancel()
        {
            TempData["Error"] = "Payment was cancelled.";
            return View();
        }
    }
}
