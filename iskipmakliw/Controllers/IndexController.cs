using Microsoft.AspNetCore.Mvc;

namespace iskipmakliw.Controllers
{
    public class IndexController : Controller
    {
        public IActionResult Index(int? id)
        {
            if(id == null)
            {
                return View();
            }
            else
            {
                return View(id);
            }
        }
        public IActionResult Product(int? id)
        {
            if (id == null)
            {
                return View();
            }
            else
            {
                return View(id);
            }
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult BecomeSeller()
        {
            return View();
        }
        public IActionResult Account()
        {
            return View();
        }
        public IActionResult Orders()
        {
            return View();
        }
        public IActionResult Cart()
        {
            return View();
        }
    }
}
