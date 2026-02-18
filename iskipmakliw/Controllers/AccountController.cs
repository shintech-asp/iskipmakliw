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

            var user = _context.Users.Include(u => u.UserDetails).FirstOrDefault(u => u.Email == users.Email);

            if (user != null)
            {

                var payments = _context.Payments.Where(p => p.UsersId == user.Id).OrderByDescending(p => p.Id).FirstOrDefault();
                var hasher = new PasswordHasher<Users>();
                var result = hasher.VerifyHashedPassword(user, user.Password, users.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    if(user.Role != "Admin")
                    {
                        if (!user.IsEmailVerified)
                        {
                            TempData["Email"] = user.Email;
                            TempData["Error"] = "Please verify your email before logging in.";
                            return RedirectToAction("VerifyEmail", "Index");
                        }
                    }
                    
                    var claims = new List<Claim>
                {
                    new Claim("UsersId", user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("ContactNumber", user.ContactNumber ?? ""),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("Status", user.UserDetails?.Status ?? "N/A"),
                    new Claim("PaymentStatus", payments?.Status ?? "N/A"),
                    new Claim("isSeller", (user.UserDetails != null).ToString())

                };

                    var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                    var principal = new ClaimsPrincipal(identity);

                    // 🔹 Sign in with cookie auth
                    await HttpContext.SignInAsync("MyCookieAuth", principal);

                    // 🔹 Redirect based on role
                    TempData["Success"] = "Logged in!";
                switch (user.Role)
                    {
                        case "Admin":
                            return RedirectToAction("Index", "Admin");
                        case "Customer":
                            return RedirectToAction("Index", "Home");
                        case "Seller":
                            return RedirectToAction("Index", "Seller");
                        case "Rider":
                            return RedirectToAction("Index", "Rider");
                    }
                }

            }

            TempData["Error"] = "Invalid username or password";
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
