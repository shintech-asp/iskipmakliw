using iskipmakliw.Data;
using iskipmakliw.Models;
using iskipmakliw.Models.ViewModels;
using iskipmakliw.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iskipmakliw.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        ApplicationDbContext _context;
        EmailService _emailService;
        public AdminController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
                    Image = p.ProductVariants.FirstOrDefault().ProductImage
                })
                .ToList();

            var sellers = _context.Users
                .Where(u => u.Username.Contains(query))
                .Select(u => new SellerViewModel
                {
                    SellerId = u.Id,
                    SellerName = u.Username,
                    ProfileImage = "~/src/assets/img/OakMartLogo.png"
                })
                .ToList();

            ViewBag.Query = query;

            return View(new SearchResultViewModel
            {
                Products = products,
                Sellers = sellers
            });
        }
        [HttpGet]
        public IActionResult Terms()
        {
            var data = _context.Terms.FirstOrDefault();
            return View(data);
        }
        [HttpPost]
        public IActionResult Terms(string Content)
        {
            var data = _context.Terms.FirstOrDefault();
            if (data == null)
            {
                data = new Terms { Content = Content };
                _context.Terms.Add(data);
            }
            else
            {
                data.Content = Content;
                _context.Terms.Update(data);
            }
            _context.SaveChanges();
            TempData["Success"] = "Terms and Conditions successfully updated!";
            return View(data);
        }
        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                // Recent Subscribers (last 7 days)
                RecentSubscribers = await _context.Users
                    .Include(s => s.Subscription.Where(u => u.Expiration >= DateTime.UtcNow))
                            .ThenInclude(s => s.Plans)
                    .Include(s => s.UserDetails)
                    .Where(s => s.DateCreated >= DateTime.UtcNow.AddDays(-7) && s.Role == "Seller")
                    .OrderByDescending(s => s.DateCreated)
                    .Take(10)
                    .ToListAsync(),

                // Purchased Plans
                PurchasedPlans = await _context.Users
                    .Include(s => s.Subscription)
                            .ThenInclude(s => s.Plans)
                    .Include(s => s.UserDetails)
                    .Where(s => s.Role == "Seller")
                    .OrderByDescending(s => s.DateCreated)
                    .Take(10)
                    .ToListAsync(),

                // Rider Applications
                RiderApplications = await _context.Users
                    .Include(r => r.UserDetails)
                    .Where(s => s.Role == "Rider")
                    .OrderByDescending(r => r.DateCreated)
                    .Take(10)
                    .ToListAsync(),

                // Recently Registered Customers
                RecentCustomers = await _context.Users
                    .Where(u => u.DateCreated >= DateTime.UtcNow.AddDays(-7) && u.Role == "Customer")
                    .OrderByDescending(u => u.DateCreated)
                    .Take(10)
                    .ToListAsync(),

                // Statistics
                TotalSubscribers = await _context.Users.Where(s => s.Role == "Seller").CountAsync(),
                ActiveSubscriptions = await _context.Payments
                        .Where(p => p.DueDate >= DateTime.UtcNow && p.Status == "Paid")
                        .GroupBy(p => p.UsersId)
                        .CountAsync(),
                PendingRiders = await _context.Users.Include(r => r.UserDetails)
                    .Where(r => r.UserDetails.Status == "Pending")
                    .CountAsync(),
                TotalCustomers = await _context.Users
                    .Where(u => u.Role == "Customer")
                    .CountAsync(),

                // Monthly Subscriber Data (for graph)
                MonthlySubscriberData = await GetMonthlySubscriberData()
            };

            return View(viewModel);
        }

        private async Task<List<MonthlySubscriberData>> GetMonthlySubscriberData()
        {
            var data = new List<MonthlySubscriberData>();
            var today = DateTime.UtcNow;

            for (int i = 11; i >= 0; i--)
            {
                var monthStart = today.AddMonths(-i);
                monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

                var count = await _context.Users.Include(r => r.UserDetails)
                    .Where(s => s.DateCreated >= monthStart && s.DateCreated <= monthEnd)
                    .CountAsync();

                data.Add(new MonthlySubscriberData
                {
                    Month = monthStart.ToString("MMM"),
                    Count = count
                });
            }

            return data;
        }
        public IActionResult Users()
        {
            var data = _context.Users.ToList();
            return View(data);
        }
        public IActionResult Sellers()
        {
            var payments = _context.Payments
                            .Include(p => p.Users)
                                .ThenInclude(u => u.UserDetails)
                            .Include(p => p.Users)
                                .ThenInclude(u => u.Subscription)
                                    .ThenInclude(u => u.Plans)
                            .ToList();

            return View(payments);
        }
        public IActionResult Riders()
        {
            var payments = _context.Users
                                .Include(u => u.UserDetails)
                                    .ThenInclude(u => u.VehicleImages)
                            .Where(u => u.Role == "Rider")
                            .ToList();

            return View(payments);
        }
        public IActionResult RiderReview(int Id)
        {
            ViewBag.Id = Id;
            var data = _context.Users
                            .Include(u => u.UserDetails)
                                    .ThenInclude(u => u.VehicleImages)
                            .Where(u => u.Id == Id)
                            .FirstOrDefault();
            return View(data);
        }
        public IActionResult SellerReview(int Id)
        {
            ViewBag.Id = Id;
            var data = _context.Payments
                            .Include(p => p.Users)
                            .ThenInclude(u => u.UserDetails)
                            //.ThenInclude(u => u.Plans)
                            .Where(u => u.Users.Id == Id)
                            .FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        public IActionResult SellerReview(string Status, int Ids, string? DeclinedReason)
        {
            var user = _context.Users
                .Include(u => u.UserDetails)
                .Where(u => u.Id == Ids)
                .FirstOrDefault();

            user.UserDetails.Status = Status;
            user.UserDetails.DeclinedReason = DeclinedReason;
            _context.SaveChanges();

            // Send status email
            _emailService.SendSellerStatusEmail(user.Email, Status, DeclinedReason, user.Username);

            TempData["Success"] = "Status successfully changed!";
            return RedirectToAction("Index");
        }
    }
}
