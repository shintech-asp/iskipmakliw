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
        public IActionResult Signup()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Signup(Users users, string Confirm)
        {
            users.Role = "Customer";
            ModelState.Remove("Role");
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
