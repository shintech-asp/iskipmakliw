using iskipmakliw.Data;
using iskipmakliw.Models.DTO;
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
                         .Include(dp => dp.PurchasedProduct)
                            .ThenInclude(p => p.Users)
                                .ThenInclude(u => u.UserDetails)
                         .Where(dp => dp.RiderId == null && dp.Id == Id)
                         .FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        public IActionResult OrderView(int Id, int? usersId)
        {
            usersId = int.Parse(User.FindFirst("UsersId").Value);
            var data = _context.DeliverProduct.Where(u => u.Id == Id).FirstOrDefault();
            data.RiderId = usersId;
            data.Status = "Accepted";
            data.AcceptedOn = DateTime.Now;
            _context.DeliverProduct.Update(data);
            _context.SaveChanges();
            TempData["Success"] = "Delivery accepted!";
            return RedirectToAction("Delivery");
        }
        public IActionResult Delivery()
        {
            var usersId = int.Parse(User.FindFirst("UsersId").Value);
            var data = _context.DeliverProduct
                         .Include(dp => dp.PurchasedProduct)
                             .ThenInclude(pp => pp.Billings)
                         .Include(dp => dp.PurchasedProduct)
                             .ThenInclude(pp => pp.ProductVariants)
                                 .ThenInclude(pv => pv.Product)
                                     .ThenInclude(p => p.Users)
                                         .ThenInclude(u => u.UserDetails)
                         .Include(dp => dp.PurchasedProduct)
                            .ThenInclude(p => p.Users)
                                .ThenInclude(u => u.UserDetails)
                         .Where(dp => dp.RiderId == usersId && dp.DeliveredOn == null)
                         .FirstOrDefault();
            return View(data);
        }
        [HttpPost]
        public IActionResult UpdateDriversPosition([FromBody] DeliverUpdateModel model)
        {
            var data = _context.DeliverProduct.FirstOrDefault(u => u.Id == model.Id);
            if (data != null)
            {
                data.DriversLat = model.Lat;
                data.DriversLong = model.Long;
                _context.DeliverProduct.Update(data);
                _context.SaveChanges();
                return Json(new { success = true, message = "Driver position updated" });
            }

            return Json(new { success = false, message = "Delivery not found" });
        }
        [HttpPost]
        public IActionResult Delivery(int Id, int Type)
        {
            if(Id != null)
            {
                if (Type == 1)
                {
                    var data = _context.DeliverProduct.Where(u => u.Id == Id).FirstOrDefault();
                    data.PickUpOn = DateTime.Now;
                    _context.DeliverProduct.Update(data);
                    _context.SaveChanges();
                    TempData["Success"] = "Pick up success!";
                }
                else if (Type == 2)
                {
                    var data = _context.DeliverProduct.Where(u => u.Id == Id).FirstOrDefault();
                    data.DropOffOn = DateTime.Now;
                    _context.DeliverProduct.Update(data);
                    var transaction = _context.PurchasedProduct.Where(u => u.Id == data.PurchasedProductId).FirstOrDefault();
                    transaction.TransactionStatus = "In transit";
                    _context.PurchasedProduct.Update(transaction);
                    _context.SaveChanges();
                    TempData["Success"] = "Drop off success!";

                }
                else if (Type == 3)
                {
                    var data = _context.DeliverProduct.Where(u => u.Id == Id).FirstOrDefault();
                    data.DeliveredOn = DateTime.Now;
                    data.Status = "Delivered";
                    _context.DeliverProduct.Update(data);
                    var transaction = _context.PurchasedProduct.Where(u => u.Id == data.PurchasedProductId).FirstOrDefault();
                    transaction.TransactionStatus = "To rate";
                    _context.PurchasedProduct.Update(transaction);
                    _context.SaveChanges();
                    TempData["Success"] = "Yay! Delivery Complete!";

                }
            }
            else
            {
                TempData["Error"] = "Oops. Please select delivery first!";
            }

                return RedirectToAction("Delivery");
        }
    }
}
