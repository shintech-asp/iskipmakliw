using iskipmakliw.Data;
using iskipmakliw.Models;
using iskipmakliw.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

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
            try
            {
                var userId = int.Parse(User.FindFirst("UsersId")?.Value ?? "0");

                var data = _context.Product
                    .Select(p => new ClientViewModel
                    {
                        ProductId = p.Id,
                        Name = p.Name,
                        SellerName = p.Users.Username,
                        SellerId = p.UsersId,
                        Price = p.ProductVariants
                            .OrderBy(v => v.Price)
                            .Select(v => v.Price)
                            .FirstOrDefault(),
                        FirstImage = p.Gallery
                            .OrderBy(g => g.Id)
                            .Select(g => g.Image)
                            .FirstOrDefault(),
                        ProductVariants = p.ProductVariants.ToList()
                    })
                    .ToList();

                return View(data);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public IActionResult Account() => SafeView();
        public IActionResult Orders() => SafeView();
        public IActionResult Cart() => SafeView();
        public IActionResult Customization() => SafeView();

        [HttpGet]
        public IActionResult BecomeSeller() => SafeView();

        [HttpPost]
        public IActionResult BecomeSeller(UserDetails user)
        {
            try
            {
                ModelState.Remove("Status");
                user.Status = "Pending";
                user.UsersId = int.Parse(User.FindFirst("UsersId")?.Value ?? "0");

                ModelState.Remove("UsersId");
                ModelState.Remove("Plans");
                ModelState.Remove("Users");

                // Convert GovernmentIdFile
                if (user.GovernmentIdFile != null)
                {
                    using var ms = new MemoryStream();
                    user.GovernmentIdFile.CopyTo(ms);
                    user.GovernmentId = ms.ToArray();
                }

                // Convert CapturedIdFile
                if (user.CapturedIdFile != null)
                {
                    using var ms = new MemoryStream();
                    user.CapturedIdFile.CopyTo(ms);
                    user.CapturedId = ms.ToArray();
                }

                if (ModelState.IsValid)
                {
                    _context.UserDetails.Add(user);

                    // Update role to Seller
                    var roleChange = _context.Users.Find(user.UsersId);
                    if (roleChange != null)
                    {
                        roleChange.Role = "Seller";

                        var claims = new List<Claim>
                        {
                            new Claim("UsersId", roleChange.Id.ToString()),
                            new Claim(ClaimTypes.Name, roleChange.Username),
                            new Claim(ClaimTypes.Email, roleChange.Email),
                            new Claim("ContactNumber", roleChange.ContactNumber ?? ""),
                            new Claim(ClaimTypes.Role, roleChange.Role),
                            new Claim("Status", roleChange.UserDetails?.Status ?? "N/A")
                        };

                        var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                        var principal = new ClaimsPrincipal(identity);
                        HttpContext.SignInAsync("MyCookieAuth", principal);
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
                                UsersId = user.UsersId,
                                DueDate = DateTime.Now.AddMonths(1)
                            });
                        }
                        else if (user.Subscription == "Yearly")
                        {
                            var discountedPrice = plan.Price - (plan.Price * (plan.Discount / 100.0));
                            _context.Payments.Add(new Payments
                            {
                                Amount = discountedPrice * 12,
                                PaymentDetails = "Subscription",
                                Status = "Pending",
                                UsersId = user.UsersId,
                                DueDate = DateTime.Now.AddMonths(1)
                            });
                        }
                    }

                    _context.SaveChanges();

                    TempData["success"] = "Your account created successfully.";
                    return RedirectToAction("Index", "Seller");
                }

                TempData["error"] = "There was an error with your submission. Please try again.";
                return View(user);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public IActionResult Product(int Id, int SellerId)
        {
            try
            {
                var product = _context.Product
                              .Include(u => u.ProductVariants.Where(u => u.isArchive == null))
                              .ThenInclude(u => u.Gallery.Where(u => u.ProductVariants.isArchive == null))
                              .FirstOrDefault(p => p.Id == Id && p.UsersId == SellerId);

                if (product == null)
                {
                    TempData["error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                return View(product);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // 🔹 Helper for actions that only return a View
        private IActionResult SafeView()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }
    }
}
