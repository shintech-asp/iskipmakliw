using iskipmakliw.Data;
using iskipmakliw.Filters;
using iskipmakliw.Models;
using iskipmakliw.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iskipmakliw.Controllers
{
    [RedirectIfAuthenticated]
    public class IndexController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IndexController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
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
                return RedirectToAction("Error");
            }
        }

        public IActionResult Product()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Signup()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public IActionResult Signup(Users users, string Confirm)
        {
            try
            {
                users.Role = "Customer";
                ModelState.Remove("Role");

                if (ModelState.IsValid && (Confirm == users.Password))
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

                    return RedirectToAction("Login", "Account");
                }
                else if (Confirm != users.Password)
                {
                    ViewBag.Error = "Password and Confirm Password do not match";
                }

                return View(users);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        public IActionResult Account()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }
    }
}
