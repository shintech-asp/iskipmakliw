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
            int usersId = int.Parse(User.FindFirst("UsersId")?.Value);
            var data = _context.Cart
                        .Include(u => u.ProductVariants)
                            .ThenInclude(u => u.Product)
                                .ThenInclude(u => u.Users)
                                    .ThenInclude(u => u.UserDetails)
                        .Include(u => u.Users)
                        .Where(u => u.UsersId == usersId)
                        .ToList();
            var groupedCart = data
                            .GroupBy(c => c.ProductVariants.Product.Users.Username)
                            .ToDictionary(g => g.Key, g => g.ToList());

            return View(groupedCart);
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

        [HttpPost]
        public IActionResult Product(int Id, Cart model)
        {
            int usersId = int.Parse(User.FindFirst("UsersId")?.Value);
            if (model.ProductVariantsId == 0 || model.ProductVariantsId == null)
            {
                TempData["Error"] = "Please select a product variant.";
                return RedirectToAction("Product", new { Id = Id });
            }
            var checkCart = _context.Cart.Where(u => u.ProductVariantsId == model.ProductVariantsId && u.UsersId == usersId).FirstOrDefault();
            if(checkCart == null)
            {
                var cart = new Cart
                {
                    UsersId = usersId,
                    ProductVariantsId = model.ProductVariantsId,
                    Quantity = model.Quantity

                };

                _context.Cart.Add(cart);
                TempData["Success"] = "Product added to the cart!";
            }
            else
            {
                checkCart.Quantity += model.Quantity;
                TempData["Success"] = "Product added to the cart!";
                _context.Cart.Update(checkCart);
            }

            _context.SaveChanges();
            return RedirectToAction("Cart");
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
        [HttpPost]
        public async Task<IActionResult> Profile(int submissionType, Users user, Billings billing, string PaymentType, string PaymentContactNumber, string HolderName, string? NewPassword, string? ConfirmPassword)
        {
            int usersId = int.Parse(User.FindFirst("UsersId")?.Value);
            var hasher = new PasswordHasher<Users>();
            if (submissionType == 1)
            {
                var userDetails = _context.Users
                                .Include(u => u.Payments)
                                .Include(u => u.UserDetails)
                                .FirstOrDefault(u => u.Id == usersId);
                if (user.Username != null && user.Email != null && user.ContactNumber != null)
                {
                    userDetails.Username = user.Username;
                    userDetails.Email = user.Email;
                    userDetails.ContactNumber = user.ContactNumber;

                    if (user.Password != null)
                    {
                        if (!string.IsNullOrEmpty(NewPassword) &&
                            NewPassword == ConfirmPassword &&
                            hasher.VerifyHashedPassword(userDetails, userDetails.Password, user.Password) == PasswordVerificationResult.Success)
                        {
                            userDetails.Password = hasher.HashPassword(userDetails, NewPassword);
                        }
                        else if (NewPassword != ConfirmPassword)
                        {
                            TempData["Error"] = "Password do not match!";
                        }
                        else if(hasher.VerifyHashedPassword(userDetails, userDetails.Password, user.Password) == PasswordVerificationResult.Failed)
                        {
                            TempData["Error"] = "Old password incorrect!";
                        }
                    }
                    
                    TempData["Success"] = "Details successfully changed!";
                    _context.Users.Update(userDetails);
                    _context.SaveChanges();
                    var claims = new List<Claim>
                {
                    new Claim("UsersId", userDetails.Id.ToString()),
                    new Claim(ClaimTypes.Name, userDetails.Username),
                    new Claim(ClaimTypes.Email, userDetails.Email),
                    new Claim("ContactNumber", userDetails.ContactNumber ?? ""),
                    new Claim(ClaimTypes.Role, userDetails.Role),
                    new Claim("Status", userDetails.UserDetails?.Status ?? "N/A"),
                    new Claim("PaymentStatus", userDetails.Payments?.FirstOrDefault()?.Status ?? "N/A")
                };

                    var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("MyCookieAuth", principal);
                }
                else
                {
                    TempData["Error"] = "Please fill up all the fields required!";
                }
                
            }else if(submissionType == 2)
            {
                if(billing.Zip != null 
                    && billing.LandMark != null 
                    && billing.Latitude != null 
                    && billing.Longitude != null 
                    && billing.Name != null 
                    && billing.ContactNumber != null 
                    && billing.Address != null 
                    && billing.City != null 
                    && billing.Country != null)
                {
                    billing.UsersId = usersId;
                    _context.Billings.Add(billing);
                    _context.SaveChanges();
                    TempData["Success"] = "Billing address added successfully!";
                }
                else
                {
                    TempData["Error"] = "Please fill up all the fields required!";
                }
            }else if(submissionType == 3)
            {
                if(PaymentType != null
                    && PaymentContactNumber != null
                    && HolderName != null)
                {
                    var paymentMethod = new PaymentMethod
                    {
                        Type = PaymentType,
                        Number = PaymentContactNumber,
                        HolderName = HolderName,
                        UsersId = usersId
                    };
                    _context.PaymentMethod.Add(paymentMethod);
                    _context.SaveChanges();
                    TempData["Success"] = "Payment method added successfully!";
                }
                else
                {
                    TempData["Error"] = "Please fill up all the fields required!";
                }
            }
                return RedirectToAction("Index");
        }
    }
}
