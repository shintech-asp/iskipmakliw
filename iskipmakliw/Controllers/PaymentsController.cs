using iskipmakliw.Data;
using iskipmakliw.Models;
using iskipmakliw.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;

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
            int? userId = int.Parse(User.FindFirst("UsersId")?.Value);
            if (userId == null)
                return RedirectToAction("Logout", "Account");

            var payments = _context.Users
                            .Include(u => u.UserDetails)
                            .Include(u => u.Subscription)
                                .ThenInclude(u => u.Plans)
                            .Include(u => u.Payments)
                            .Include(u => u.Product)
                            .Include(u => u.Billings)
                            .Where(u => u.Id == userId)
                            .FirstOrDefault();
            if(payments == null)
            {
                return RedirectToAction("Index", "Seller");
            }
            return View(payments);
        }

        [HttpPost]
        public async Task<IActionResult> PayNow(decimal totalAmount, string productNames, string paymentIds)
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

            // save for later validation
            TempData["PaymentIds"] = paymentIds;
            TempData["SessionId"] = sessionId;

            return Redirect(checkoutUrl);
        }


        public async Task<IActionResult> Success()
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);
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
                
                await _context.SaveChangesAsync();
            }
            var user = _context.Users
                        .Include(u => u.UserDetails)
                        .Include(u => u.Payments)
                        .FirstOrDefault(u => u.Id == userId);
            var claims = new List<Claim>
                {
                    new Claim("UsersId", user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("ContactNumber", user.ContactNumber ?? ""),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("Status", user.UserDetails?.Status ?? "N/A"),
                    new Claim("PaymentStatus", user?.Payments.FirstOrDefault().Status ?? "N/A")
                };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            // 🔹 Sign in with cookie auth
            await HttpContext.SignInAsync("MyCookieAuth", principal);
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
