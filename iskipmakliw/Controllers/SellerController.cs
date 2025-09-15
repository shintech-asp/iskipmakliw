using iskipmakliw.Data;
using iskipmakliw.Models;
using iskipmakliw.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;

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
            int usersId = int.Parse(User.FindFirst("UsersId")?.Value);
            var user = _context.Payments.Include(p => p.Users)
                            .ThenInclude(u => u.UserDetails)
                            .ThenInclude(u => u.Plans)
                            .Where(u => u.Users.Id == usersId)
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
        [HttpPost]
        public IActionResult ProductAdd(Product product)
        {
            var filter = _context.Product.Where(u => u.Name == product.Name).FirstOrDefault();
            if (ModelState.IsValid)
            {
                if (filter == null)
                {
                    var newProduct = _context.Product.Add(product);
                    _context.SaveChanges();
                    TempData["success"] = "Product added successfully.";
                    return View(product);
                }
                else
                {
                    TempData["error"] = "Product already exists.";
                    return View();
                }
            }
            else
            {
                TempData["error"] = "Fill up all the fields";
                return View();
            }
                
        }
        public IActionResult ProductEdit()
        {
            return View();
        }
        public IActionResult ProductList()
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);

            var data = _context.Product
                        .Include(u => u.ProductVariants)
                        .Include(g => g.Gallery)
                        .Where(d => d.pUsersId == userId)
                        .ToList();

            return View(data);
        }
        public IActionResult ProductDetails(int Id)
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);

            var data = _context.ProductVariants
                           .Include(u => u.Product)
                           .ThenInclude(g => g.Gallery)
                           .Where(d => d.Product.Id == Id && d.Product.pUsersId == userId)
                           .ToList();

            var productName = _context.Product
                              .Where(n => n.pUsersId == userId && n.Id == Id)
                              .FirstOrDefault();

            var ProductDetailsViewModel = new ProductDetailsViewModel
            {
                Product = productName,
                ProductVariants = data
            };
            return View(ProductDetailsViewModel);
        }
        [HttpPost]
        public IActionResult ProductDetails(ProductDetailsViewModel model)
        {
            //if (string.IsNullOrEmpty(model.VariantsJson))
            //{
            //    TempData["Error"] = "No variants were submitted.";
            //    return RedirectToAction("ProductDetails", new { id = model.Product.Id });
            //}

            //var variants = JsonConvert.DeserializeObject<List<ProductVariants>>(model.VariantsJson);

            //foreach (var v in variants)
            //{
            //    v.ProductId = model.Product.Id;
            //    _context.ProductVariants.Add(v);
            //}

            //_context.SaveChanges();

            //TempData["Success"] = "Variants added successfully.";
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
