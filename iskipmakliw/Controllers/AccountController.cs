using iskipmakliw.Data;
using iskipmakliw.Filters;
using iskipmakliw.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace iskipmakliw.Controllers
{
    public class AccountController : Controller
    {
        ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [RedirectIfAuthenticated]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [RedirectIfAuthenticated]
        public async Task<IActionResult> Login(Users users)
        {
            if (string.IsNullOrEmpty(users.Email) || string.IsNullOrEmpty(users.Password))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(users);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == users.Email);

            if (user != null)
            {
                var hasher = new PasswordHasher<Users>();
                var result = hasher.VerifyHashedPassword(user, user.Password, users.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    // 🔹 Set session BEFORE redirect
                    HttpContext.Session.SetInt32("UsersId", user.Id);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("ContactNumber", user.ContactNumber);
                    HttpContext.Session.SetString("Role", user.Role);

                    // 🔹 Add cookie authentication (optional but recommended)
                    var claims = new List<Claim>
            {
                new Claim("UsersId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

                    var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("MyCookieAuth", principal);

                    // 🔹 Redirect based on role
                    switch (user.Role)
                    {
                        case "Admin":
                            return RedirectToAction("Index", "Admin");
                        case "Customer":
                            return RedirectToAction("Index", "Home");
                        case "Seller":
                            return RedirectToAction("Index", "Seller");
                    }
                }
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(users);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Index");
        }

    }
}
