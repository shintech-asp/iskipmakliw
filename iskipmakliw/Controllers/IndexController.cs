using iskipmakliw.Data;
using iskipmakliw.Filters;
using iskipmakliw.Helper;
using iskipmakliw.Migrations;
using iskipmakliw.Models;
using iskipmakliw.Models.DTO;
using iskipmakliw.Models.ViewModels;
using iskipmakliw.Services;
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
        private EmailService emailService = new EmailService();
        public IndexController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var data = _context.Product
                .Where(u => u.ProductVariants.Any())
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
        public IActionResult Rider(Models.UserDetails userDetails, List<IFormFile> VehicleImagesFile, Users users, string Confirm)
        {
            ModelState.Remove("Role");
            ModelState.Remove("Carts");
            ModelState.Remove("Users");
            ModelState.Remove("CapturedIdPath");
            ModelState.Remove("VerificationCode");
            ModelState.Remove("GovernmentIdPath");
            ModelState.Remove("DeedOfSaleFile");
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
                    // Save GovernmentIdFile to disk
                    if (userDetails.ORFile != null && userDetails.ORFile.Length > 0)
                    {
                        string ORFileName = $"or_{Guid.NewGuid()}{Path.GetExtension(userDetails.ORFile.FileName)}";
                        string ORFilePath = Path.Combine(uploadPath, ORFileName);

                        using (var stream = new FileStream(ORFilePath, FileMode.Create))
                        {
                            userDetails.GovernmentIdFile.CopyTo(stream);
                        }

                        // Save relative path in DB
                        userDetails.OR = $"/uploads/{ORFileName}";
                    }
                    // Save GovernmentIdFile to disk
                    if (userDetails.CRFile != null && userDetails.CRFile.Length > 0)
                    {
                        string CRFileName = $"cr_{Guid.NewGuid()}{Path.GetExtension(userDetails.CRFile.FileName)}";
                        string CRFilePath = Path.Combine(uploadPath, CRFileName);

                        using (var stream = new FileStream(CRFilePath, FileMode.Create))
                        {
                            userDetails.GovernmentIdFile.CopyTo(stream);
                        }

                        // Save relative path in DB
                        userDetails.CR = $"/uploads/{CRFileName}";
                    }

                    string otpCode = OTPHelper.GenerateOTP();
                    var hasher = new PasswordHasher<Users>();
                    users.Password = hasher.HashPassword(users, users.Password);
                    users.Role = "Rider";
                    users.IsEmailVerified = false;
                    users.VerificationCode = otpCode;
                    users.CodeCreatedAt = DateTime.UtcNow;
                    users.LastCodeSentAt = DateTime.UtcNow;
                    _context.Users.Add(users);
                    _context.SaveChanges();
                    userDetails.Status = "Pending";
                    userDetails.UsersId = users.Id;
                    _context.UserDetails.Add(userDetails);
                    _context.SaveChanges();
                    if (VehicleImagesFile != null && VehicleImagesFile.Any())
                    {
                        foreach (var file in VehicleImagesFile)
                        {
                            var fileName = $"gov_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            var vahicleImages = new Models.VehicleImages
                            {
                                ImagePath = $"/uploads/{fileName}",
                                UserDetailsId = userDetails.Id
                            };
                            _context.VehicleImages.Add(vahicleImages);
                            _context.SaveChanges();
                        }
                    }
                    bool emailSent = emailService.SendVerificationCode(users.Email, otpCode);
                    if (emailSent)
                    {
                        TempData["Email"] = users.Email;
                        TempData["Success"] = "Registration successful! We've sent a 4-digit verification code to your email.";
                        return RedirectToAction("VerifyEmail");
                    }
                    else
                    {
                        TempData["Error"] = "Registration successful but failed to send verification code. Please try resending.";
                        TempData["Email"] = users.Email;
                        return RedirectToAction("VerifyEmail");
                    }
                }
            }
            TempData["Error"] = "Please enter a value for each field";
            return View();


        }
        [HttpGet]
        public IActionResult Signup()
        {
            var data = _context.Terms.FirstOrDefault();
            return View(data);
        }
        [HttpPost]
        public IActionResult Signup(Users users, string Confirm)
        {
            users.Role = "Customer";
            ModelState.Remove("Role");
            ModelState.Remove("Carts");
            ModelState.Remove("IsEmailVerified");
            ModelState.Remove("VerificationCode");
            ModelState.Remove("CodeCreatedAt");
            ModelState.Remove("LastCodeSentAt");
            if (ModelState.IsValid &&(Confirm == users.Password))
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == users.Email);
                if (existingUser != null)
                {
                    TempData["Error"] = "Email already in use";
                    return View(users);
                }
                string otpCode = OTPHelper.GenerateOTP();
                var hasher = new PasswordHasher<Users>();
                users.Password = hasher.HashPassword(users, users.Password);
                users.IsEmailVerified = false;
                users.VerificationCode = otpCode;
                users.CodeCreatedAt = DateTime.UtcNow;
                users.LastCodeSentAt = DateTime.UtcNow;

                _context.Users.Add(users);
                _context.SaveChanges();
                bool emailSent = emailService.SendVerificationCode(users.Email, otpCode);
                if (emailSent)
                {
                    TempData["Email"] = users.Email;
                    TempData["Success"] = "Registration successful! We've sent a 4-digit verification code to your email.";
                    return RedirectToAction("VerifyEmail");
                }
                else
                {
                    TempData["Error"] = "Registration successful but failed to send verification code. Please try resending.";
                    TempData["Email"] = users.Email;
                    return RedirectToAction("VerifyEmail");
                }
            }else if(Confirm != users.Password)
            {
                TempData["Error"] = "Password and Confirm Password do not match";
            }
            else
            {
                TempData["Error"] = "Fill up all required details";
            }
                return View(users);
        }
        // GET: VerifyEmail
        public ActionResult VerifyEmail()
        {
            if (TempData["Email"] != null)
            {
                ViewBag.Email = TempData["Email"].ToString();
                TempData.Keep("Email");
            }

            ViewBag.HideChrome = true;
            return View();
        }

        // POST: VerifyEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyEmail(string Email, string Code)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Code))
            {
                ViewBag.HideChrome = true;
                TempData["Error"] = "Email and Code are required.";
                ViewBag.Email = Email;
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == Email && !u.IsEmailVerified);

            if (user == null)
            {
                TempData["Error"] = "User not found or already verified.";
                ViewBag.Email = Email;
                ViewBag.HideChrome = true;
                return View();
            }

            // Check if code expired
            if (OTPHelper.IsOTPExpired(user.CodeCreatedAt, 10))
            {
                TempData["Error"] = "Verification code has expired. Please request a new one.";
                ViewBag.Email = Email;
                ViewBag.ShowResendButton = true;
                ViewBag.HideChrome = true;
                return View();
            }

            // Verify code
            if (user.VerificationCode != Code)
            {
                TempData["Error"] = "Invalid verification code.";
                ViewBag.Email = Email;
                ViewBag.HideChrome = true;
                return View();
            }

            // Mark as verified
            user.IsEmailVerified = true;
            user.CodeCreatedAt = null;
            user.LastCodeSentAt = null;
            _context.SaveChanges();

            TempData["Success"] = "Email verified successfully! You can now login.";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public JsonResult ResendVerificationCode(string Email)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == Email && !u.IsEmailVerified);

                if (user == null)
                {
                    ViewBag.Email = Email;
                    ViewBag.HideChrome = true;
                    return Json(new { success = false, message = "User not found or already verified." });
                }

                // Check cooldown period (2 minutes)
                if (!OTPHelper.CanResendOTP(user.LastCodeSentAt, 2))
                {
                    var remaining = OTPHelper.GetRemainingCooldown(user.LastCodeSentAt, 2);
                    ViewBag.Email = Email;
                    ViewBag.HideChrome = true;
                    return Json(new
                    {
                        success = false,
                        message = $"Please wait {remaining.Minutes}:{remaining.Seconds:D2} before requesting a new code.",
                        remainingSeconds = (int)remaining.TotalSeconds
                    });
                }

                // Generate new OTP
                string newCode = OTPHelper.GenerateOTP();
                user.VerificationCode = newCode;
                user.CodeCreatedAt = DateTime.UtcNow;
                user.LastCodeSentAt = DateTime.UtcNow;
                _context.SaveChanges();

                // Send email
                bool emailSent = emailService.SendVerificationCode(user.Email, newCode);

                if (emailSent)
                {
                    ViewBag.Email = Email;
                    ViewBag.HideChrome = true;
                    return Json(new { success = true, message = "Verification code sent! Please check your email." });
                }
                else
                {
                    ViewBag.Email = Email;
                    ViewBag.HideChrome = true;
                    return Json(new { success = false, message = "Failed to send email. Please try again." });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Email = Email;
                ViewBag.HideChrome = true;
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }
        public IActionResult Account()
        {
            return View();
        }
       
    }
}
