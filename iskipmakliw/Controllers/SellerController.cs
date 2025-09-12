using iskipmakliw.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iskipmakliw.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        ApplicationDbContext _context;
        public SellerController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var user = _context.Payments.Include(p => p.Users)
                            .ThenInclude(u => u.UserDetails)
                            .ThenInclude(u => u.Plans)
                            .Where(u => u.Users.Id == HttpContext.Session.GetInt32("UsersId"))
                            .FirstOrDefault();
            return View(user);
        }
        public IActionResult Chats()
        {
            return View();
        }
        public IActionResult ProductAdd()
        {
            return View();
        }
        public IActionResult ProductEdit()
        {
            return View();
        }
        public IActionResult ProductList()
        {
            return View();
        }
        public IActionResult Users()
        {
            return View();
        }
        public IActionResult Reports()
        {
            return View();
        }
        public IActionResult StoreSettings()
        {
            return View();
        }
        public IActionResult Orders()
        {
            return View();
        }
    }
}
