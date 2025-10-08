using iskipmakliw.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iskipmakliw.Controllers
{
    public class RiderController : Controller
    {

        ApplicationDbContext _context;
        public RiderController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Orders()
        {
            var data = _context.DeliverProduct
                         .Include(dp => dp.PurchasedProduct)
                             .ThenInclude(pp => pp.Billings)
                         .Include(dp => dp.PurchasedProduct)
                             .ThenInclude(pp => pp.ProductVariants)
                                 .ThenInclude(pv => pv.Product)
                                     .ThenInclude(p => p.Users)
                                         .ThenInclude(u => u.UserDetails)
                         .Where(dp => dp.RiderId == null)
                         .ToList();
            return View(data);
        }
        public IActionResult OrderView(int Id)
        {
            var data = _context.DeliverProduct
                         .Include(dp => dp.PurchasedProduct)
                             .ThenInclude(pp => pp.Billings)
                         .Include(dp => dp.PurchasedProduct)
                             .ThenInclude(pp => pp.ProductVariants)
                                 .ThenInclude(pv => pv.Product)
                                     .ThenInclude(p => p.Users)
                                         .ThenInclude(u => u.UserDetails)
                         .Where(dp => dp.RiderId == null)
                         .FirstOrDefault();
            return View(data);
        }
    }
}
