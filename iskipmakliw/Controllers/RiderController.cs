using iskipmakliw.Data;
using iskipmakliw.Models;
using iskipmakliw.Models.DTO;
using iskipmakliw.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iskipmakliw.Controllers
{
    public class RiderController : Controller
    {

        ApplicationDbContext _context;
        EmailService _emailService;
        public RiderController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            var usersId = int.Parse(User.FindFirst("UsersId").Value);
            var data = _context.DeliverProduct
                    .Include(u => u.PurchasedProduct)
                        .ThenInclude(u => u.ProductVariants)
                            .ThenInclude(u => u.Product)
                                .ThenInclude(u => u.Users)
                                    .ThenInclude(u => u.UserDetails)
                    .Include(u => u.PurchasedProduct)
                        .ThenInclude(u => u.CustomizationOrders)
                            .ThenInclude(u => u.Users)
                                .ThenInclude(u => u.UserDetails)
                    .Where(u => u.RiderId == usersId)
                    .ToList();
            ViewBag.Status = _context.Users
                            .Include(u => u.UserDetails)
                            .Where(u => u.Id == usersId).FirstOrDefault();
            return View(data);
        }
        public IActionResult Remit()
        {
            var data = _context.DeliverProduct.Where(u => !u.isRemitted && u.RiderId == int.Parse(User.FindFirst("UsersId").Value)).ToList();
            if(data.Count == 0)
            {
                TempData["Error"] = "No remittance available";

                return Json(new { response = data });
            }
            foreach (var item in data)
            {
                item.isRemitted = true;
                item.RemittedOn = DateTime.Now; 
            }
            _context.SaveChanges();
            TempData["Success"] = "Cash remitted successfully";

            return Json(new { response = data });

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
                         .Include(dp => dp.PurchasedProduct)
                            .ThenInclude(pp => pp.CustomizationOrders)
                                .ThenInclude(pp => pp.Sellers)
                                    .ThenInclude(pp => pp.UserDetails)
                         .Include(dp => dp.PurchasedProduct)
                            .ThenInclude(pp => pp.CustomizationOrders)
                                .ThenInclude(pp => pp.Users)
                                    .ThenInclude(pp => pp.UserDetails)
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
                .Include(dp => dp.PurchasedProduct)
                    .ThenInclude(pp => pp.CustomizationOrders)
                        .ThenInclude(pp => pp.Sellers)
                            .ThenInclude(pp => pp.UserDetails)
                .Include(dp => dp.PurchasedProduct)
                    .ThenInclude(pp => pp.CustomizationOrders)
                        .ThenInclude(pp => pp.Users)
                            .ThenInclude(pp => pp.UserDetails)
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
                        .Include(dp => dp.PurchasedProduct)
                            .ThenInclude(pp => pp.CustomizationOrders)
                                .ThenInclude(pp => pp.Sellers)
                                    .ThenInclude(pp => pp.UserDetails)
                        .Include(dp => dp.PurchasedProduct)
                            .ThenInclude(pp => pp.CustomizationOrders)
                                .ThenInclude(pp => pp.Users)
                                    .ThenInclude(pp => pp.UserDetails)
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
        public IActionResult Delivery(int Id, int Type, IFormFile? ImageFile)
        {
            if(Id != null)
            {
                if (Type == 1)
                {
                    var data = _context.DeliverProduct.Where(u => u.Id == Id).FirstOrDefault();
                    data.PickUpOn = DateTime.Now;
                    _context.DeliverProduct.Update(data);
                    _context.SaveChanges();
                    var transaction = _context.PurchasedProduct.Include(u => u.Users).Where(u => u.Id == data.PurchasedProductId).FirstOrDefault();
                    _emailService.SendSuccessDropOffEmail(transaction.Users.Email, transaction.Id);

                    TempData["Success"] = "Pick up success!";
                }
                else if (Type == 2)
                {
                    var data = _context.DeliverProduct.Where(u => u.Id == Id).FirstOrDefault();
                    data.DropOffOn = DateTime.Now;
                    _context.DeliverProduct.Update(data);
                    var transaction = _context.PurchasedProduct.Include(u => u.Users).Where(u => u.Id == data.PurchasedProductId).FirstOrDefault();
                    transaction.TransactionStatus = "In transit";
                    _context.PurchasedProduct.Update(transaction);
                    _context.SaveChanges();
                    
                    TempData["Success"] = "Drop off success!";

                }
                else if (Type == 3)
                {
                    if(ImageFile != null)
                    {
                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        string path = null;
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        // Save GovernmentIdFile to disk
                        if (ImageFile != null)
                        {
                            string govFileName = $"gov_{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                            string govFilePath = Path.Combine(uploadPath, govFileName);

                            using (var stream = new FileStream(govFilePath, FileMode.Create))
                            {
                                ImageFile.CopyTo(stream);
                            }

                            // Save relative path in DB
                            path = $"/uploads/{govFileName}";
                        }
                        var data = _context.DeliverProduct.Where(u => u.Id == Id).FirstOrDefault();
                        data.DeliveredOn = DateTime.Now;
                        data.Status = "Delivered";
                        data.ProofImage = path;
                        _context.DeliverProduct.Update(data);
                        var transaction = _context.PurchasedProduct.Where(u => u.Id == data.PurchasedProductId).FirstOrDefault();
                        transaction.TransactionStatus = "To rate";
                        transaction.PaymentStatus = "Paid";
                        _context.PurchasedProduct.Update(transaction);
                        var customization = _context.CustomizationOrders.Where(u => u.Id == data.PurchasedProduct.CustomizationOrdersId).FirstOrDefault();
                        if (customization != null)
                        {
                            customization.TransactionStatus = "Completed";
                            _context.CustomizationOrders.Update(customization);
                            _emailService.SendSuccessDeliveryEmail(transaction.Users.Email, customization.Id, "custom");
                        }
                        else
                        {
                            _emailService.SendSuccessDeliveryEmail(transaction.Users.Email, transaction.Id, "normal");
                        }
                            _context.SaveChanges();

                        
                        TempData["Success"] = "Yay! Delivery Complete!";
                    }
                    else
                    {
                        TempData["Error"] = "Oops. Please submit an image first!";
                    }

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
