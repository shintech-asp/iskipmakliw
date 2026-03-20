using iskipmakliw.Data;
using iskipmakliw.Models;
using iskipmakliw.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;

namespace iskipmakliw.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymongo _paymongo;
        ApplicationDbContext _context;
        EmailService _emailService;
        public PaymentsController(IPaymongo paymongo, ApplicationDbContext context, EmailService emailService)
        {
            _paymongo = paymongo;
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Checkout(int Id)
        {
            int? userId = int.Parse(User.FindFirst("UsersId")?.Value);
            if (userId == null)
                return RedirectToAction("Logout", "Account");

            var payments = _context.Users
                            .Include(u => u.UserDetails)
                            .Include(u => u.Subscription)
                                .ThenInclude(u => u.Plans)
                            .Include(u => u.Payments)
                            .Include(u => u.Product)
                            .Include(u => u.Billings)
                            .Where(u => u.Id == userId)
                            .FirstOrDefault();
            if(payments == null)
            {
                return RedirectToAction("Index", "Seller");
            }
            return View(payments);
        }

        [HttpPost]
        public async Task<IActionResult> PayNow(decimal totalAmount, string productNames, string paymentIds)
        {
            var name = User.FindFirst(ClaimTypes.Name)?.Value ?? "Guest";
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "guest@example.com";
            var contact = User.FindFirst("ContactNumber")?.Value ?? "0000000000";
            var sessionJson = await _paymongo.CreateCheckoutSessionService(
                totalAmount,
                "PHP",
                name,
                email,
                contact,
                productNames,
                "gcash"
            );

            dynamic session = JsonConvert.DeserializeObject(sessionJson);
            string checkoutUrl = session?.data?.attributes?.checkout_url;
            string sessionId = session?.data?.id;

            if (string.IsNullOrEmpty(checkoutUrl) || string.IsNullOrEmpty(sessionId))
            {
                TempData["Error"] = "Unable to create payment session.";
                return RedirectToAction("Checkout");
            }

            // save for later validation
            TempData["PaymentIds"] = paymentIds;
            TempData["SessionId"] = sessionId;

            return Redirect(checkoutUrl);
        }
        [HttpPost]
        public async Task<IActionResult> PayProduct(List<int> cartSelect, string paymentMethod, int? paymentMethodOnline, int billings, int shippingFee)
        {
            var usersId = int.Parse(User.FindFirst("UsersId")?.Value ?? "0");
            var user = _context.Users.Find(usersId);
            if (paymentMethodOnline != null && billings != null)
            {
                if (cartSelect == null || !cartSelect.Any())
                {
                    TempData["Error"] = "No items selected for checkout.";
                    return RedirectToAction("Cart");
                }


                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Cart");
                }

                // ✅ Get selected cart items
                var selectedCarts = _context.Cart
                    .Include(c => c.ProductVariants)
                    .ThenInclude(pv => pv.Product)
                    .Where(c => cartSelect.Contains(c.Id) && c.UsersId == usersId)
                    .ToList();
                var paymentMethodData = _context.PaymentMethod.Where(u => u.UsersId == usersId && u.Id == paymentMethodOnline).FirstOrDefault();
                if (!selectedCarts.Any())
                {
                    TempData["Error"] = "No valid cart items found.";
                    return RedirectToAction("Cart");
                }

                // ✅ Compute total
                double? totalAmount = 0;
                List<(string name, double price, int quantity)> productDetails = new();

                foreach (var item in selectedCarts)
                {
                    var price = item.ProductVariants.Price;
                    var discount = item.ProductVariants.Discount;
                    var quantity = item.Quantity;
                    double? discountedPrice;
                    double? total;
                    if (discount != null)
                    {
                        discountedPrice = price - (price * discount / 100);
                        total = discountedPrice;
                    }
                    else
                    {
                        total = price;
                    }

                    totalAmount += total;
                    productDetails.Add((
                        $"{item.ProductVariants.Product.Name}",
                        (double)total,
                        quantity
                    ));
                }

                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "guest@example.com";
                long totalInCentavos = (long)(totalAmount);
                var sessionJson = await _paymongo.CreateCheckoutSession(
                    totalInCentavos,
                    "PHP",
                    email,
                    User.FindFirst(ClaimTypes.Email)?.Value ?? "guest@example.com",
                    paymentMethodData.Number,
                    productDetails,
                    paymentMethodData.Type,
                    shippingFee
                );

                dynamic session = JsonConvert.DeserializeObject(sessionJson);
                string checkoutUrl = session?.data?.attributes?.checkout_url;
                string sessionId = session?.data?.id;

                if (string.IsNullOrEmpty(checkoutUrl) || string.IsNullOrEmpty(sessionId))
                {
                    TempData["Error"] = "Unable to create payment session.";
                    return RedirectToAction("Checkout");
                }

                // ✅ Save for later validation
                TempData["CartIds"] = string.Join(",", cartSelect);
                TempData["ShippingFee"] = shippingFee;
                TempData["BillingsId"] = billings;
                TempData["SessionId"] = sessionId;
                TempData["PaymentMethod"] = "Online";

                return Redirect(checkoutUrl);
            }
            else if(paymentMethodOnline == null && billings != null && paymentMethod == "COD")
            {
                var cartsData = _context.Cart.Include(p => p.ProductVariants).Where(p => cartSelect.Contains(p.Id)).ToList();
                foreach (var payment in cartsData)
                {
                    var price = payment.ProductVariants.Price;
                    var quantity = payment.Quantity;
                    var discount = payment.ProductVariants.Discount;
                    double? discountedPrice;
                    double? total;
                    if (discount != null)
                    {
                        discountedPrice = price - (price * discount / 100);
                        total = discountedPrice * quantity + 20;
                    }
                    else
                    {
                        total = price + 20;
                        discountedPrice = price;
                    }

                    var purchasedProduct = new PurchasedProduct
                    {
                        UsersId = payment.UsersId,
                        ProductVariantsId = payment.ProductVariantsId,
                        Quantity = payment.Quantity,
                        Price = total,
                        ShippingFee = shippingFee,
                        Source = "ProductVariants",
                        PaymentStatus = "Pending",
                        PaymentMethod = "Cash on Delivery",
                        PurchasedDate = DateTime.UtcNow.AddHours(8),
                        BillingsId = billings
                    };
                    _context.PurchasedProduct.Add(purchasedProduct);
                    var updateProduct = _context.ProductVariants.FirstOrDefault(u => u.Id == payment.ProductVariantsId);
                    updateProduct.Quantity -= payment.Quantity;
                    _context.ProductVariants.Update(updateProduct);
                    var cartRemove = _context.Cart.FirstOrDefault(u => u.Id == payment.Id);
                    _context.Cart.Remove(cartRemove);
                    await _context.SaveChangesAsync();
                    _emailService.SendSuccessEmail(user.Email, purchasedProduct.Id, "Cash on Delivery");
                }
                TempData["Success"] = "Purchase successfuly!";
                await _context.SaveChangesAsync();
                return RedirectToAction("Cart", "Home");
            }
            else
            {
                TempData["Error"] = "Purchase error! Please select a payment method or billing address";
                return RedirectToAction("Cart", "Home");
            }

        }
        [HttpPost]
        public async Task<IActionResult> Pay3dProduct(long amount, int billings, int PaymentMethodId, int CustomizationOrdersId)
        {
            try
            {
                var usersId = int.Parse(User.FindFirst("UsersId")?.Value ?? "0");
                var user = _context.Users.Find(usersId);
                var paymentMethodData = _context.PaymentMethod.Where(u => u.UsersId == usersId && u.Id == PaymentMethodId).FirstOrDefault();
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "guest@example.com";

                // Validate amount (in cents)
                if (amount <= 0)
                {
                    return Json(new { success = false, message = "Invalid payment amount" });
                }

                // productDetails with PHP price
                List<(string name, decimal price, int quantity)> productDetails = new()
        {
            ("Cuztomization", amount / 100m, 1)
        };

                var sessionJson = await _paymongo.Create3dCheckoutSession(
                    amount, // Pass cents directly as long
                    "PHP",
                    email,
                    email,
                    paymentMethodData.Number,
                    productDetails,
                    paymentMethodData.Type
                );

                dynamic session = JsonConvert.DeserializeObject(sessionJson);
                string checkoutUrl = session?.data?.attributes?.checkout_url;
                string sessionId = session?.data?.id;

                if (string.IsNullOrEmpty(checkoutUrl) || string.IsNullOrEmpty(sessionId))
                {
                    return Json(new { success = false, message = "Unable to create payment session." });
                }

                // Store TempData["Amount"] as string (PHP format)
                TempData["BillingsId"] = billings;
                TempData["SessionId"] = sessionId;
                TempData["Amount"] = (amount / 100m).ToString("F2"); // e.g., "12999.00"
                TempData["CustomizationOrdersId"] = CustomizationOrdersId;
                TempData["PaymentMethod"] = "E-wallet";

                // Return JSON with redirectUrl (avoids CORS)
                return Json(new { success = true, redirectUrl = checkoutUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        public async Task<IActionResult> SuccessPurchaseProduct()
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);
            var sessionId = TempData["SessionId"]?.ToString();
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session not found.");
            }

            // Ask PayMongo about this checkout session
            var sessionJson = await _paymongo.GetCheckoutSession(sessionId);
            dynamic session = JsonConvert.DeserializeObject(sessionJson);

            // payments is an array; get first payment
            var payments = session?.data?.attributes?.payments as IEnumerable<dynamic>;
            var first = payments?.FirstOrDefault();
            string status = first?.attributes?.status;

            // Update DB
            var cartIds = TempData["CartIds"]?.ToString()?.Split(',').Select(int.Parse).ToList();
            var billingId = TempData["BillingsId"]?.ToString();
            var shippingFee = TempData["ShippingFee"]?.ToString();
            
            if (cartIds != null && cartIds.Any())
            {
                // Update payment records
                var cartsData = _context.Cart.Include(p => p.ProductVariants).Where(p => cartIds.Contains(p.Id)).ToList();
                foreach (var payment in cartsData)
                {
                    var price = payment.ProductVariants.Price;
                    var quantity = payment.Quantity;
                    var discount = payment.ProductVariants.Discount;
                    double? discountedPrice;
                    double? total;
                    if (discount != null)
                    {
                        discountedPrice = price - (price * discount / 100);
                        total = discountedPrice * quantity;
                    }
                    else
                    {
                        total = price;
                        discountedPrice = price;
                    }
                    var purchasedProduct = new PurchasedProduct
                    {
                        UsersId = payment.UsersId,
                        ProductVariantsId = payment.ProductVariantsId,
                        Quantity = payment.Quantity,
                        Price = total,
                        ShippingFee = double.Parse(shippingFee),
                        Source = "ProductVariants",
                        PaymentStatus = status switch
                        {
                            "paid" => "Paid",
                            "succeeded" => "Paid",
                            "failed" => "Failed",
                            _ => "Pending"
                        },
                        PaymentMethod = "Online",
                        PurchasedDate = DateTime.UtcNow.AddHours(8),
                        BillingsId = int.Parse(billingId)
                    };
                    _context.PurchasedProduct.Add(purchasedProduct);
                    var updateProduct = _context.ProductVariants.FirstOrDefault(u => u.Id == payment.ProductVariantsId);
                    updateProduct.Quantity -= payment.Quantity;
                    _context.ProductVariants.Update(updateProduct);
                    var cartRemove = _context.Cart.FirstOrDefault(u => u.Id == payment.Id);
                    _context.Cart.Remove(cartRemove);

                    if (status == "paid" || status == "succeeded")
                    {
                        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                        _emailService.SendSuccessEmail(user.Email, purchasedProduct.Id, "Online payment");
                    }
                }
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "Payment successful!";
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Success3dPurchaseProduct()
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);
            var sessionId = TempData["SessionId"]?.ToString();
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session not found.");
            }

            // Ask PayMongo about this checkout session
            var sessionJson = await _paymongo.GetCheckoutSession(sessionId);
            dynamic session = JsonConvert.DeserializeObject(sessionJson);

            // payments is an array; get first payment
            var payments = session?.data?.attributes?.payments as IEnumerable<dynamic>;
            var first = payments?.FirstOrDefault();
            string status = first?.attributes?.status;

            var billingId = TempData["BillingsId"]?.ToString();
            var amount = TempData["Amount"]?.ToString();
            var CustomizationOrdersId = TempData["CustomizationOrdersId"]?.ToString();
            if (string.IsNullOrEmpty(billingId) || string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(CustomizationOrdersId))
            {
                return BadRequest("Invalid payment data");
            }

            // Parse amount to double
            if (!double.TryParse(amount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedAmount))
            {
                return BadRequest("Invalid amount format");
            }
            var purchasedProduct = new PurchasedProduct
            {
                UsersId = userId,
                Quantity = 1,
                Price = parsedAmount,
                CustomizationOrdersId = int.Parse(CustomizationOrdersId),
                Source = "CustomizationOrders",
                PaymentStatus = status switch
                {
                    "paid" => "Paid",
                    "succeeded" => "Paid",
                    "failed" => "Failed",
                    _ => "Pending"
                },
                PaymentMethod = "E-wallet",
                PurchasedDate = DateTime.UtcNow.AddHours(8),
                BillingsId = int.Parse(billingId)
            };
             _context.PurchasedProduct.Add(purchasedProduct);
             await _context.SaveChangesAsync();
            var customizationProduct = _context.CustomizationOrders.Find(int.Parse(CustomizationOrdersId));
            customizationProduct.PaymentStatus = "Paid";
            _context.CustomizationOrders.Update(customizationProduct);
            await _context.SaveChangesAsync();
            var userData = _context.Users.Find(userId);
            _emailService.SendSuccessEmail(userData.Email, purchasedProduct.Id, "Online payment");
            TempData["Success"] = "Payment successful!";
            return RedirectToAction("Chat", "Home");
        }
        public async Task<IActionResult> Success()
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);
            var sessionId = TempData["SessionId"]?.ToString();
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session not found.");
            }

            // Ask PayMongo about this checkout session
            var sessionJson = await _paymongo.GetCheckoutSession(sessionId);
            dynamic session = JsonConvert.DeserializeObject(sessionJson);

            // payments is an array; get first payment
            var payments = session?.data?.attributes?.payments as IEnumerable<dynamic>;
            var first = payments?.FirstOrDefault();
            string status = first?.attributes?.status;

            // Update DB
            var paymentIds = TempData["PaymentIds"]?.ToString()?.Split(',').Select(int.Parse).ToList();

            if (paymentIds != null && paymentIds.Any())
            {
                // Update payment records
                var paymentsData = _context.Payments.Where(p => paymentIds.Contains(p.Id)).ToList();
                foreach (var payment in paymentsData)
                {
                    payment.Status = status switch
                    {
                        "paid" => "Paid",
                        "succeeded" => "Paid",
                        "failed" => "Failed",
                        _ => "Pending"
                    };
                }
                
                await _context.SaveChangesAsync();
            }
            var user = _context.Users
                        .Include(u => u.UserDetails)
                        .Include(u => u.Payments)
                        .FirstOrDefault(u => u.Id == userId);
            var claims = new List<Claim>
                {
                    new Claim("UsersId", user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("ContactNumber", user.ContactNumber ?? ""),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("Status", user.UserDetails?.Status ?? "N/A"),
                    new Claim("PaymentStatus", user?.Payments.FirstOrDefault().Status ?? "N/A")
                };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            // 🔹 Sign in with cookie auth
            await HttpContext.SignInAsync("MyCookieAuth", principal);
            TempData["Success"] = "Payment successful!";
            return View();
        }



        public IActionResult Cancel()
        {
            TempData["Error"] = "Payment was cancelled.";
            return View();
        }
    }
}
