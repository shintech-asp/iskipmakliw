using Microsoft.AspNetCore.Mvc;

namespace iskipmakliw.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Users()
        {
            return View();
        }
        public IActionResult Sellers()
        {
            return View();
        }
    }
}
