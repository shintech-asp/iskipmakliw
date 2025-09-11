using iskipmakliw.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var user = _context.UserDetails.FirstOrDefault(u => u.UsersId == 6);
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
