using iskipmakliw.Data;
using iskipmakliw.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iskipmakliw.Filters;
using System.Security.Claims;

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
            return View();
        }
        public IActionResult Product()
        {
            return View();
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
