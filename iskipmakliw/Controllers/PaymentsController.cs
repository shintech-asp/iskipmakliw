using iskipmakliw.Data;
using iskipmakliw.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace iskipmakliw.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymongo _paymongo;
        private readonly ApplicationDbContext _context;

        public PaymentsController(IPaymongo paymongo, ApplicationDbContext context)
        {
            _paymongo = paymongo;
            _context = context;
        }

        [HttpGet]
        public IActionResult Checkout(int Id)
        {
            try
            {
                int? userId = int.TryParse(User.FindFirst("UsersId")?.Value, out int parsedId) ? parsedId : (int?)null;
                if (userId == null)
                    return RedirectToAction("Logout", "Account");

                var payments = _context.Payments
                    .Include(p => p.Users)
                        .ThenInclude(u => u.UserDetails)
                        .ThenInclude(u => u.Plans)
                    .Where(p => p.UsersId == userId && p.Status == "Pending" && p.Id == Id && p.DueDate >= DateTime.Now)
                    .ToList();

                if (payments == null || !payments.Any())
                    return RedirectToAction("Index", "Seller");

                return View(payments);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong while loading checkout.";
                return RedirectToAction("Index", "Seller");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PayNow(decimal totalAmount, string productNames, string paymentIds)
        {
            try
            {
                var name = User.FindFirst(ClaimTypes.Name)?.Value ?? "Guest";
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "guest@example.com";
                var contact = User.FindFirst("ContactNumber")?.Value ?? "0000000000";

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

                TempData["PaymentIds"] = paymentIds;
                TempData["SessionId"] = sessionId;

                return Redirect(checkoutUrl);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong while processing payment.";
                return RedirectToAction("Checkout");
            }
        }

        public async Task<IActionResult> Success()
        {
            try
            {
                var sessionId = TempData["SessionId"]?.ToString();
                if (string.IsNullOrEmpty(sessionId))
                    return BadRequest("Session not found.");

                var sessionJson = await _paymongo.GetCheckoutSession(sessionId);
                dynamic session = JsonConvert.DeserializeObject(sessionJson);

                var payments = session?.data?.attributes?.payments as IEnumerable<dynamic>;
                var first = payments?.FirstOrDefault();
                string status = first?.attributes?.status;

                var paymentIds = TempData["PaymentIds"]?.ToString()?.Split(',').Select(int.Parse).ToList();

                if (paymentIds != null && paymentIds.Any())
                {
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
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong while confirming your payment.";
                return RedirectToAction("Checkout");
            }
        }

        public IActionResult Cancel()
        {
            try
            {
                TempData["Error"] = "Payment was cancelled.";
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Checkout");
            }
        }
    }
}
