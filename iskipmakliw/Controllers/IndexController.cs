using iskipmakliw.Data;
using iskipmakliw.Filters;
using iskipmakliw.Models;
using iskipmakliw.Models.DTO;
using iskipmakliw.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace iskipmakliw.Controllers
{
    [RedirectIfAuthenticated]
    public class IndexController : Controller
    {
        ApplicationDbContext _context;
        public IndexController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
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
        public IActionResult Product(int Id, int SellerId)
        {
            var product = _context.Product
                        .Include(p => p.ProductVariants.Where(u => u.isArchive == null))
                        .FirstOrDefault(p => p.Id == Id);
            var ratings = _context.Ratings
                            .Include(u => u.ProductVariants)
                                .ThenInclude(u => u.Product)
                                    .ThenInclude(u => u.Users)
                                        .ThenInclude(u => u.UserDetails)
                            .Include(u => u.PurchasedProduct)
                                .ThenInclude(u => u.ProductVariants)
                                    .ThenInclude(u => u.Product)
                                        .ThenInclude(u => u.Users)
                                            .ThenInclude(u => u.UserDetails)
                            .Include(u => u.Users)
                                .ThenInclude(u => u.UserDetails)
                            .Where(u => u.ProductVariants.ProductId == Id)
                            .ToList();
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
            var Product = new ProductCustomerViewModel
            {
                Product = product,
                Ratings = ratings
            };
            return View(Product);

        }
        [HttpGet]
        public IActionResult Rider()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Rider(UserDetails userDetails, Users users, string Confirm)
        {
            ModelState.Remove("Role");
            ModelState.Remove("Carts");
            ModelState.Remove("Users");
            ModelState.Remove("CapturedIdPath");
            ModelState.Remove("GovernmentIdPath");
            if (ModelState.IsValid)
            {
                if (Confirm != users.Password)
                {
                    TempData["Error"] = "Password do not match!";
                    return View();
                }
                else
                { // Define upload folder
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Save GovernmentIdFile to disk
                    if (userDetails.GovernmentIdFile != null && userDetails.GovernmentIdFile.Length > 0)
                    {
                        string govFileName = $"gov_{Guid.NewGuid()}{Path.GetExtension(userDetails.GovernmentIdFile.FileName)}";
                        string govFilePath = Path.Combine(uploadPath, govFileName);

                        using (var stream = new FileStream(govFilePath, FileMode.Create))
                        {
                            userDetails.GovernmentIdFile.CopyTo(stream);
                        }

                        // Save relative path in DB
                        userDetails.GovernmentIdPath = $"/uploads/{govFileName}";
                    }

                    // Save CapturedIdFile to disk
                    if (userDetails.CapturedIdFile != null && userDetails.CapturedIdFile.Length > 0)
                    {
                        string capFileName = $"cap_{Guid.NewGuid()}.png";
                        string capFilePath = Path.Combine(uploadPath, capFileName);

                        using (var stream = new FileStream(capFilePath, FileMode.Create))
                        {
                            userDetails.CapturedIdFile.CopyTo(stream);
                        }

                        // Save relative path in DB
                        userDetails.CapturedIdPath = $"/uploads/{capFileName}";
                    }

                    var hasher = new PasswordHasher<Users>();
                    users.Password = hasher.HashPassword(users, users.Password);
                    users.Role = "Rider";
                    _context.Users.Add(users);
                    _context.SaveChanges();
                    userDetails.Status = "Pending";
                    userDetails.UsersId = users.Id;
                    _context.UserDetails.Add(userDetails);
                    _context.SaveChanges();
                    TempData["Success"] = "Account created!";
                    return View();
                }
            }
            TempData["Error"] = "Please enter a value for each field";
            return View();


        }
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Signup(Users users, string Confirm)
        {
            users.Role = "Customer";
            ModelState.Remove("Role");
            ModelState.Remove("Carts");
            if (ModelState.IsValid &&(Confirm == users.Password))
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == users.Email);
                if (existingUser != null)
                {
                    ViewBag.Error = "Email already in use";
                    return View(users);
                }
                var hasher = new PasswordHasher<Users>();
                users.Password = hasher.HashPassword(users, users.Password);
                _context.Users.Add(users);
                _context.SaveChanges();
                TempData["Success"] = "Account successfully created!";
                return RedirectToAction("Login", "Account");
            }else if(Confirm != users.Password)
            {
                ViewBag.Error = "Password and Confirm Password do not match";
            }
            return View(users);
        }
        
        public IActionResult Account()
        {
            return View();
        }
       
    }
}
