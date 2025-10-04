using iskipmakliw.Data;
using iskipmakliw.Models;
using iskipmakliw.Models.DTO;
using iskipmakliw.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);
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
                    Image = p.ProductVariants.FirstOrDefault().ProductImage

                })
                .ToList();

            return View(data);
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
            var data = _context.Plans.ToList();
            return View(data);
        }
        [HttpPost]
        public IActionResult BecomeSeller(UserDetails user, int PlansId)
        {
            ModelState.Remove("Status");
            user.Status = "Pending";
            user.UsersId = int.Parse(User.FindFirst("UsersId").Value);

            ModelState.Remove("UsersId");
            ModelState.Remove("Plans");
            ModelState.Remove("Users");
            ModelState.Remove("GovernmentIdPath");
            ModelState.Remove("CapturedIdPath");

            // Define upload folder
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Save GovernmentIdFile to disk
            if (user.GovernmentIdFile != null && user.GovernmentIdFile.Length > 0)
            {
                string govFileName = $"gov_{Guid.NewGuid()}{Path.GetExtension(user.GovernmentIdFile.FileName)}";
                string govFilePath = Path.Combine(uploadPath, govFileName);

                using (var stream = new FileStream(govFilePath, FileMode.Create))
                {
                    user.GovernmentIdFile.CopyTo(stream);
                }

                // Save relative path in DB
                user.GovernmentIdPath = $"/uploads/{govFileName}";
            }

            // Save CapturedIdFile to disk
            if (user.CapturedIdFile != null && user.CapturedIdFile.Length > 0)
            {
                string capFileName = $"cap_{Guid.NewGuid()}.png";
                string capFilePath = Path.Combine(uploadPath, capFileName);

                using (var stream = new FileStream(capFilePath, FileMode.Create))
                {
                    user.CapturedIdFile.CopyTo(stream);
                }

                // Save relative path in DB
                user.CapturedIdPath = $"/uploads/{capFileName}";
            }

            if (ModelState.IsValid)
            {
                // Save user details
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

                // Get plan and add payment/subscription
                var plan = _context.Plans.FirstOrDefault(u => u.Id == PlansId);
                if (plan != null)
                {
                    double? amount = plan.Price - (plan.Price * (plan.Discount / 100.0));

                    _context.Payments.Add(new Payments
                    {
                        Amount = amount,
                        PaymentDetails = "Subscription",
                        Status = "Pending",
                        UsersId = user.UsersId,
                        DueDate = DateTime.Now.AddMonths(1)
                    });

                    _context.Subscription.Add(new Models.Subscription
                    {
                        UsersId = user.UsersId,
                        PlansId = PlansId,
                        Expiration = null,
                        Status = "Pending",
                    });
                }

                _context.SaveChanges();

                return RedirectToAction("Index", "Seller");
            }

            TempData["Error"] = "There was an error with your submission. Please try again.";
            return View(user);
        }

        public IActionResult Product(int Id, int SellerId)
        {
            var product = _context.Product
                        .Include(p => p.ProductVariants.Where(u => u.isArchive == null))
                        .FirstOrDefault(p => p.Id == Id);

            var variantDtos = product.ProductVariants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Color = v.Color,
                Size = v.Dimension,
                Price = v.Price,
                Stock = v.Quantity,
                ProductImage = v.ProductImage,
                Discount = v.Discount
            }).ToList();

            ViewBag.ProductVariantsJson = JsonSerializer.Serialize(variantDtos);
            return View(product);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
