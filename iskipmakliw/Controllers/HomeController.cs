using iskipmakliw.Data;
using iskipmakliw.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace iskipmakliw.Controllers
{
    [Authorize(Roles = "Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Account()
        {
            return View();
        }
        public IActionResult Orders()
        {
            return View();
        }
        public IActionResult Cart()
        {
            return View();
        }
        public IActionResult Customization()
        {
            return View();
        }
        [HttpGet]
        public IActionResult BecomeSeller()
        {
            return View();
        }
        [HttpPost]
        public IActionResult BecomeSeller(UserDetails user)
        {
            ModelState.Remove("Status");
            user.Status = "Pending";
            user.UsersId = _httpContextAccessor.HttpContext.Session.GetInt32("UsersId") ?? 0;

            ModelState.Remove("UsersId");
            ModelState.Remove("Plans");
            ModelState.Remove("Users");
            if (user.GovernmentIdFile != null)
            {
                using var ms = new MemoryStream();
                user.GovernmentIdFile.CopyTo(ms);
                user.GovernmentId = ms.ToArray();
            }

            // Convert CapturedIdFile to byte[]
            if (user.CapturedIdFile != null)
            {
                using var ms = new MemoryStream();
                user.CapturedIdFile.CopyTo(ms);
                user.CapturedId = ms.ToArray();
            }

            if (ModelState.IsValid)
            {
                // Save user details
                _context.UserDetails.Add(user);

                // Update role to Seller
                var roleChange = _context.Users.Find(user.UsersId);
                if (roleChange != null)
                {
                    roleChange.Role = "Seller"; // ✅ assignment not comparison
                }

                // Get plan
                var plan = _context.Plans.FirstOrDefault(u => u.Id == user.PlansId);
                if (plan != null)
                {
                    if (user.Subscription == "Monthly")
                    {
                        _context.Payments.Add(new Payments
                        {
                            Amount = plan.Price,
                            PaymentDetails = "Subscription",
                            Status = "Pending",
                            UsersId = user.UsersId
                        });
                    }
                    else if (user.Subscription == "Yearly")
                    {
                        var discountedPrice = plan.Price - (plan.Price * (plan.Discount / 100.0));
                        _context.Payments.Add(new Payments
                        {
                            Amount = discountedPrice * 12, // yearly subscription
                            PaymentDetails = "Subscription",
                            Status = "Pending",
                            UsersId = user.UsersId
                        });
                    }
                }

                // Save everything in one go
                _context.SaveChanges();

                return RedirectToAction("Index", "Seller");
            }

            ViewBag.Error = "There was an error with your submission. Please try again.";
            return View(user);

        }

        public IActionResult Product()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
