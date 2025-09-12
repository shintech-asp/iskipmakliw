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

        public IActionResult SellerReview(int Id)
        {
            var data = _context.Payments
                            .Include(p => p.Users)
                            .ThenInclude(u => u.UserDetails)
                            .ThenInclude(u => u.Plans)
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
