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
                    product.pUsersId = int.Parse(User.FindFirst("UsersId")?.Value);
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
        public IActionResult ProductEdit(int Id)
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);
            var product = _context.ProductVariants
                        .Include(v => v.Product)
                        .Include(v => v.Gallery)
                        .FirstOrDefault(v => v.Id == Id);

            if (product != null)
            {
                product.Gallery = _context.Gallery
                    .Where(g => g.pUsersId == userId && g.ProductId == product.Id)
                    .ToList();
            }

            return View(product);
        }
        [HttpPost]
        public IActionResult ProductEdit(ProductVariants product, string ButtonType, int Id)
        {
            ModelState.Remove("Product");
            if (ModelState.IsValid)
            {
                if(ButtonType == "Save")
                {
                    var existing = _context.ProductVariants.FirstOrDefault(p => p.Id == Id);

                    if (existing != null)
                    {
                        // Update existing
                        existing.ProductId = product.ProductId;
                        existing.Material = product.Material;
                        existing.Dimension = product.Dimension;
                        existing.Color = product.Color;
                        existing.Price = product.Price;
                        existing.Quantity = product.Quantity;
                        existing.Discount = product.Discount;

                        _context.ProductVariants.Update(existing);
                        TempData["success"] = "Item successfully updated";
                        _context.SaveChanges();
                    }
                }else if(ButtonType == "Remove")
                {
                    var existing = _context.ProductVariants.FirstOrDefault(p => p.Id == Id);
                    if (existing != null)
                    {
                        existing.isArchive = DateTime.Now;
                        _context.ProductVariants.Update(existing);
                        TempData["success"] = "Item successfully deleted";
                        _context.SaveChanges();
                        return RedirectToAction("Index");
                    }

                }
            }
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);

            var productVar = _context.ProductVariants
                        .Include(v => v.Product)
                        .Include(v => v.Gallery)
                        .FirstOrDefault(v => v.Id == Id);

            if (productVar != null)
            {
                productVar.Gallery = _context.Gallery
                    .Where(g => g.pUsersId == userId && g.ProductId == productVar.Id)
                    .ToList();
            }
            return View(productVar);
        }
        public IActionResult ProductList()
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);

            var data = _context.Product
                        .Include(u => u.ProductVariants.Where(g => g.isArchive == null))
                        .Include(g => g.Gallery)
                        .Where(d => d.pUsersId == userId)
                        .ToList();

            return View(data);
        }
        public IActionResult ProductDetails(int Id)
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);

            var data = _context.ProductVariants
                    .Include(v => v.Product)
                    .Where(u => u.ProductId == Id && u.Product.pUsersId == userId && u.isArchive == null)
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
        public IActionResult ProductDetails(ProductDetailsViewModel model, IFormFile Image, int Id)
        {
            if (model?.ProductDetails == null)
                return View(model);

            ModelState.Remove("Product");
            ModelState.Remove("ProductVariants");
            ModelState.Remove("ProductDetails.Product");
            ModelState.Remove("ProductDetails.ProductId");

            if (ModelState.IsValid)
            {
                ProductVariants productVariant;

                if (model.ProductDetails.Id == 0)
                {
                    productVariant = new ProductVariants
                    {
                        ProductId = Id,
                        Material = model.ProductDetails.Material,
                        Dimension = model.ProductDetails.Dimension,
                        Color = model.ProductDetails.Color,
                        Price = model.ProductDetails.Price,
                        Quantity = model.ProductDetails.Quantity,
                        Discount = model.ProductDetails.Discount
                    };
                    _context.ProductVariants.Add(productVariant);
                }
                else
                {
                    productVariant = new ProductVariants
                    {
                        Id = model.ProductDetails.Id, // make sure Id is set
                        ProductId = model.ProductDetails.Id,
                        Material = model.ProductDetails.Material,
                        Dimension = model.ProductDetails.Dimension,
                        Color = model.ProductDetails.Color,
                        Price = model.ProductDetails.Price,
                        Quantity = model.ProductDetails.Quantity,
                        Discount = model.ProductDetails.Discount
                    };
                    _context.ProductVariants.Update(productVariant);
                }

                // Save once to ensure productVariant.Id is generated
                _context.SaveChanges();

                if (Image != null)
                {
                    using var ms = new MemoryStream();
                    Image.CopyTo(ms);

                    var gallery = new Gallery
                    {
                        Image = ms.ToArray(),
                        pUsersId = int.Parse(User.FindFirst("UsersId")?.Value),
                        ProductId = productVariant.Id,
                        ImageType = "Item image"
                    };
                    _context.Gallery.Add(gallery);
                    _context.SaveChanges();
                }

                return RedirectToAction("Index", "Seller");
            }

            return View(model);
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
