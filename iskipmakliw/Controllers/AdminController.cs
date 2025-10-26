using iskipmakliw.Data;
using iskipmakliw.Models;
using iskipmakliw.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iskipmakliw.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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
        public IActionResult Index()
        {
            return View();
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
            var data = _context.Users
                            .Include(u => u.UserDetails)
                                    .ThenInclude(u => u.VehicleImages)
                            .Where(u => u.Id == Id)
                            .FirstOrDefault();
            return View(data);
        }
        public IActionResult SellerReview(int Id)
        {
            var data = _context.Payments
                            .Include(p => p.Users)
                            .ThenInclude(u => u.UserDetails)
                            //.ThenInclude(u => u.Plans)
                            .Where(u => u.Users.Id == Id)
                            .FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        public IActionResult SellerReview(string Status, int Id)
        {
            var data = _context.UserDetails.Where(u => u.UsersId == Id).FirstOrDefault();
            data.Status = Status;
            _context.SaveChanges();
            TempData["success"] = "Status successfully changed!";
            return RedirectToAction("Index");
        }
    }
}
