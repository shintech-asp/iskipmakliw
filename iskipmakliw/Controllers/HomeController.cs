using iskipmakliw.Data;
using iskipmakliw.Migrations;
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
        public IActionResult Search(string query)
        {
            var products = _context.Product
                .Where(p => p.Name.Contains(query))
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
                    Image = p.ProductVariants.FirstOrDefault().ProductImage ?? "/src/assets/img/OakMartLogo.png"
                })
                .ToList();

            var sellers = _context.Users
                .Where(u => u.Username.Contains(query))
                .Select(u => new SellerViewModel
                {
                    SellerId = u.Id,
                    Role = u.Role,
                    SellerName = u.Username,
                    ProfileImage = "/src/assets/img/OakMartLogo.png"
                })
                .Where(u => u.Role != "Admin")
                .ToList();

            ViewBag.Query = query;

            return View(new SearchResultViewModel
            {
                Products = products,
                Sellers = sellers
            });
        }
        public IActionResult Profile(int Id)
        {
            var user = _context.Users
                        .Include(u => u.PurchasedProduct)
                        .Include(u => u.UserDetails)
                        .Include(u => u.Product)
                            .ThenInclude(p => p.ProductVariants.Where(v => v.isArchive == null))
                                .ThenInclude(p => p.PurchasedProduct)
                        .Include(u => u.Product)
                            .ThenInclude(p => p.ProductVariants.Where(v => v.isArchive == null))
                                .ThenInclude(p => p.Ratings)
                        .FirstOrDefault(u => u.Id == Id);

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
                            .Where(u => u.ProductVariants.Product.UsersId == Id)
                            .ToList();
            var profile = new ProfileViewModel
            {
                Users = user,
                Ratings = ratings
            };
            return View(profile);
        }
        public IActionResult OrderDetails(int Id, int ProductId)
        {
            var toDeliver = _context.DeliverProduct
                                    .Include(u => u.PurchasedProduct)
                                        .ThenInclude(u => u.ProductVariants)
                                            .ThenInclude(u => u.Product)
                                                .ThenInclude(u => u.Users)
                                                    .ThenInclude(u => u.UserDetails)
                                    .Include(u => u.PurchasedProduct)
                                        .ThenInclude(u => u.Users)
                                            .ThenInclude(u => u.UserDetails)
                                    .Include(u => u.PurchasedProduct)
                                        .ThenInclude(pp => pp.CustomizationOrders)
                                            .ThenInclude(pp => pp.Sellers)
                                                .ThenInclude(pp => pp.UserDetails)
                                    .Include(u => u.PurchasedProduct)
                                        .ThenInclude(pp => pp.CustomizationOrders)
                                            .ThenInclude(pp => pp.Users)
                                                .ThenInclude(pp => pp.UserDetails)
                                    .Where(u => u.PurchasedProduct.Id == Id)
                                    .FirstOrDefault();
            var Rated = _context.Ratings
                            .Include(u => u.ProductVariants)
                                .ThenInclude(u => u.Product)
                            .ThenInclude(u => u.Users)
                                .ThenInclude(u => u.UserDetails)
                            .Include(u => u.PurchasedProduct)
                                .ThenInclude(u => u.Users)
                                    .ThenInclude(u => u.UserDetails)
                            .Include(u => u.PurchasedProduct)
                                .ThenInclude(pp => pp.CustomizationOrders)
                                    .ThenInclude(pp => pp.Sellers)
                                        .ThenInclude(pp => pp.UserDetails)
                            .Include(u => u.PurchasedProduct)
                                .ThenInclude(pp => pp.CustomizationOrders)
                                    .ThenInclude(pp => pp.Users)
                                        .ThenInclude(pp => pp.UserDetails)
                            .Where(u => u.PurchasedProductId == Id)
                            .FirstOrDefault();

            var timeline = new TimelineViewModel
            {
                DeliverProduct = toDeliver,
                Ratings = Rated
            };
            return View(timeline);
        }
        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);
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

            var isBilling = _context.Billings.Any(u => u.UsersId == userId);
            var isPaymentMethod = _context.PaymentMethod.Any(u => u.UsersId == userId);
            ViewBag.IsBilling = isBilling;
            ViewBag.IsPaymentMethod = isPaymentMethod;
            return View(data);
        }
        public IActionResult Cancel3d(int Id, string Reason)
        {
            var data = _context.CustomizationOrders.Find(Id);
            data.TransactionStatus = "Cancelled";
            data.SellerStatus = "Cancelled";
            data.PaymentStatus = "Cancelled";
            data.CancellationReason = Reason;
            _context.CustomizationOrders.Update(data);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        public IActionResult Account()
        {
            var usersId = int.Parse(User.FindFirst("UsersId")?.Value);
            var billings = _context.Billings.Where(u => u.UsersId == usersId).ToList();
            var paymentMethods = _context.PaymentMethod.Where(u => u.UsersId == usersId).ToList();

            var billingDetails = new BillingDetailsViewModel
            {
                Billings = billings,
                PaymentMethod = paymentMethods
            };
            return View(billingDetails);
        }
        public IActionResult Orders()
        {
            int usersId = int.Parse(User.FindFirst("UsersId")?.Value);
            var data = _context.PurchasedProduct
                        .Include(u => u.DeliverProduct)
                        .Include(u => u.ProductVariants)
                            .ThenInclude(u => u.Product)
                                .ThenInclude(u => u.Users)
                                    .ThenInclude(u => u.UserDetails)
                        .Include(u => u.Users)
                            .ThenInclude(u => u.UserDetails)
                        .Include(pp => pp.CustomizationOrders)
                            .ThenInclude(pp => pp.Sellers)
                                .ThenInclude(pp => pp.UserDetails)
                        .Include(pp => pp.CustomizationOrders)
                            .ThenInclude(pp => pp.Users)
                                .ThenInclude(pp => pp.UserDetails)
                        .Where(u => u.UsersId == usersId)
                        .ToList();
            return View(data);
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

            var billings = _context.Billings.Where(u => u.UsersId == usersId).ToList();
            var paymentMethods = _context.PaymentMethod.Where(u => u.UsersId == usersId).ToList();
            var cartViewModel = new CartViewModel
            {
                Cart = groupedCart,
                Billings = billings,
                PaymentMethod = paymentMethods
            };

            return View(cartViewModel);
        }
        [HttpGet("reverse-geocode")]
        public async Task<IActionResult> ReverseGeocode(double lat, double lon)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "OakMart/1.0");

            var url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lon}&format=json";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Geocoding service unavailable");

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
        [HttpPost]
        public IActionResult RemoveCart(int Id)
        {
            var cartItem = _context.Cart.Find(Id);
            if(cartItem != null)
            {
                _context.Cart.Remove(cartItem);
                _context.SaveChanges();
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult SubmitRating(int? Id, int purchasedId, int selectedRating, string? Review, IFormFile? ImageFile)
        {
            int usersId = int.Parse(User.FindFirst("UsersId")?.Value);
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            string path = null;
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            
            // Save GovernmentIdFile to disk
            if (ImageFile != null)
            {
                string govFileName = $"gov_{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                string govFilePath = Path.Combine(uploadPath, govFileName);

                using (var stream = new FileStream(govFilePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                // Save relative path in DB
                path = $"/uploads/{govFileName}";
            }
            if(Id != null)
            {
                var data = new Ratings
                {
                    ProductVariantsId = Id,
                    UsersId = usersId,
                    Stars = selectedRating,
                    Review = Review,
                    Image = path,
                    PurchasedProductId = purchasedId,
                };
                _context.Ratings.Add(data);
            }
            else
            {
                var data = new Ratings
                {
                    UsersId = usersId,
                    Stars = selectedRating,
                    Review = Review,
                    Image = path,
                    PurchasedProductId = purchasedId,
                };
                _context.Ratings.Add(data);
            }

            var getPurchased = _context.PurchasedProduct.Find(purchasedId);
            getPurchased.TransactionStatus = "Completed";
            _context.PurchasedProduct.Update(getPurchased);
            _context.SaveChanges();
            TempData["Success"] = $"Thank you for {selectedRating} star/s rating!";
            return RedirectToAction("Orders");
        }

        public IActionResult OrderTracking(int Id)
        {
            var usersId = int.Parse(User.FindFirst("UsersId").Value);
            var data = _context.DeliverProduct
                         .Include(dp => dp.PurchasedProduct)
                             .ThenInclude(pp => pp.Billings)
                         .Include(dp => dp.PurchasedProduct)
                             .ThenInclude(pp => pp.ProductVariants)
                                 .ThenInclude(pv => pv.Product)
                                     .ThenInclude(p => p.Users)
                                         .ThenInclude(u => u.UserDetails)
                         .Include(dp => dp.PurchasedProduct)
                            .ThenInclude(p => p.Users)
                                .ThenInclude(u => u.UserDetails)
                         .Include(dp => dp.Rider)
                            .ThenInclude(u => u.UserDetails)
                                .ThenInclude(u => u.VehicleImages)
                         .Where(dp => dp.PurchasedProductId == Id)
                         .FirstOrDefault();
            return View(data);
        }
        [HttpGet]
        public IActionResult GetDriverLocation(int Id)
        {
            var data = _context.DeliverProduct.FirstOrDefault(u => u.Id == Id);

            if (data == null)
                return Json(new { success = false, message = "Delivery not found." });

            return Json(new
            {
                success = true,
                lat = data.DriversLat,
                lng = data.DriversLong
            });
        }


        [HttpPost]
        public IActionResult ModifyQuantity(int Id, int Quantity)
        {
            var cartItem = _context.Cart.Find(Id);
            if (cartItem != null)
            {
                cartItem.Quantity = Quantity;
                _context.Cart.Update(cartItem);
                _context.SaveChanges();
            }
            return Json(new { success = true });
        }
        public IActionResult Customization(int Id)
        {
            ViewBag.Id = Id;
            var sellerModels = _context.ProductModel.Where(u => u.UsersId == Id).ToList();
            return View(sellerModels);
        }
        [HttpPost]
        public IActionResult Customization(int Id, string Model, string color, string texture, string scale, string width, string height)
        {
            if (!string.IsNullOrEmpty(Model))
            {
                var submitCustomization = new CustomizationOrders
                {
                    UsersId = int.Parse(User.FindFirst("UsersId").Value),
                    SellersId = Id,
                    Model = Model,
                    Color = color,
                    Texture = texture,
                    Scale = scale,
                    Width = width,
                    Height = height,
                    SellerStatus = "Pending",
                };
                _context.CustomizationOrders.Add(submitCustomization);
                _context.SaveChanges();

                var CustomizationChat = new CustomizationChat
                {
                    UsersId = int.Parse(User.FindFirst("UsersId").Value),
                    SellersId = Id,
                    Message = "Hello, I would like to inquire about a custom order.",
                    CustomizationOrdersId = submitCustomization.Id,
                    IsFromBuyer = true
                };
                _context.CustomizationChat.Add(CustomizationChat);
                _context.SaveChanges();

                return Json(new { success = true, message = "Customization saved successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "Please select your base model." });
            }
        }


        [HttpPost]
        public IActionResult SendMessage(int Id, string Message)
        {
            var message = _context.CustomizationChat.Where(u => u.CustomizationOrdersId == Id).FirstOrDefault();
            var CustomizationChat = new CustomizationChat
            {
                UsersId = int.Parse(User.FindFirst("UsersId").Value),
                SellersId = message.SellersId,
                Message = Message,
                CustomizationOrdersId = message.CustomizationOrdersId,
                IsFromBuyer = true
            };
            _context.CustomizationChat.Add(CustomizationChat);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpGet]
        public IActionResult GetNewMessages(int orderId, int lastMessageId)
        {
            var newMessages = _context.CustomizationChat
                .Where(c => c.CustomizationOrdersId == orderId && c.Id > lastMessageId)
                .OrderBy(c => c.Id)
                .Select(c => new {
                    c.Id,
                    c.Message,
                    c.DateSent,
                    IsCustomer = c.IsFromBuyer // ✅ renamed for frontend compatibility
                })
                .ToList();

            return Json(newMessages);
        }

        public IActionResult LoadChatPartial(int orderId)
        {
            var usersId = int.Parse(User.FindFirst("UsersId").Value);

            var messages = _context.CustomizationChat
                .Where(c => c.CustomizationOrdersId == orderId)
                .Include(c => c.CustomizationOrders)
                .Include(c => c.Sellers)
                .Include(c => c.Users)
                    .ThenInclude(u => u.Billings)
                .Include(u => u.Users)
                    .ThenInclude(u => u.PaymentMethod)
                .OrderBy(c => c.DateSent)
                .ToList();

            var data = _context.CustomizationChat
                .Where(c => c.CustomizationOrdersId == orderId).ToList();
            foreach(var data2 in data)
            {
                if (!data2.IsFromBuyer)
                {
                    data2.DateReceived = DateTime.Now;
                    _context.CustomizationChat.Update(data2);
                    _context.SaveChanges();
                }
            }
            return PartialView("_ChatConversationPartial", messages);
        }
        public IActionResult Chat()
        {
            var usersId = int.Parse(User.FindFirst("UsersId").Value);
            var data = _context.CustomizationChat
                        .Include(u => u.Users)
                            .ThenInclude(u => u.UserDetails)
                        .Include(u => u.Sellers)
                            .ThenInclude(u => u.UserDetails)
                        .Where(u => u.UsersId == usersId)
                .OrderByDescending(c => c.DateSent)
                        .ToList();
            return View(data);
        }
        [HttpGet]
        public IActionResult BecomeSeller()
        {
            var data = _context.Plans.ToList();
            return View(data);
        }
        [HttpPost]
        public IActionResult BecomeSeller(Models.UserDetails user, int PlansId)
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
                new Claim("Status", roleChange.UserDetails?.Status ?? "N/A"),
                new Claim("isSeller", (roleChange.UserDetails != null).ToString())
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

                    _context.Payments.Add(new Models.Payments
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
                Unit = v.Unit,
                Price = v.Price,
                DiscountType = v.DiscountType,
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult SwitchToSeller()
        {
            var usersId = int.Parse(User.FindFirst("UsersId").Value);
            var data = _context.Users
                        .Include(u => u.UserDetails) // ensure UserDetails is loaded
                        .FirstOrDefault(u => u.Id == usersId);
            var payments = _context.Payments.Where(p => p.UsersId == data.Id).OrderByDescending(p => p.Id).FirstOrDefault();
            if (data != null)
            {
                data.Role = "Seller";
                _context.Users.Update(data);
                _context.SaveChanges();
                var claims = new List<Claim>
                {
                new Claim("UsersId", data.Id.ToString()),
                new Claim(ClaimTypes.Name, data.Username),
                new Claim(ClaimTypes.Email, data.Email),
                new Claim("ContactNumber", data.ContactNumber ?? ""),
                new Claim(ClaimTypes.Role, data.Role),
                new Claim("Status", data.UserDetails?.Status ?? "N/A"),
                new Claim("PaymentStatus", payments?.Status ?? "N/A"),
                new Claim("isSeller", (data.UserDetails != null).ToString())
                 };

                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                var principal = new ClaimsPrincipal(identity);

                HttpContext.SignInAsync("MyCookieAuth", principal);
            }

            return Json(new { success = true });
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
                if (user.Username != null && user.ContactNumber != null)
                {
                    userDetails.Username = user.Username;
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
                    new Claim("PaymentStatus", userDetails.Payments?.FirstOrDefault()?.Status ?? "N/A"),
                    new Claim("isSeller", (userDetails.UserDetails != null).ToString())
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
                return RedirectToAction("Account");
        }
    }
}
