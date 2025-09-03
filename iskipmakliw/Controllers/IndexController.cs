using Microsoft.AspNetCore.Mvc;

namespace iskipmakliw.Controllers
{
    public class IndexController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Product()
        {
            return View();
        }
    }
}
