using iskipmakliw.Data;
using iskipmakliw.Models;
using iskipmakliw.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace iskipmakliw.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SellerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                int usersId = int.Parse(User.FindFirst("UsersId")?.Value);
                var user = _context.Payments.Include(p => p.Users)
                                .ThenInclude(u => u.UserDetails)
                                .ThenInclude(u => u.Plans)
                                .FirstOrDefault(u => u.Users.Id == usersId);

                return View(user);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public IActionResult Chats()
        {
            try { return View(); }
            catch (Exception) { return RedirectToAction("Index", "Error"); }
        }

        public IActionResult ProductAdd()
        {
            try { return View(); }
            catch (Exception) { return RedirectToAction("Index", "Error"); }
        }

        [HttpPost]
        public IActionResult ProductAdd(Product product)
        {
            try
            {
                var usersId = int.Parse(User.FindFirst("UsersId")?.Value);
                var filter = _context.Product.FirstOrDefault(u => u.Name == product.Name && u.UsersId == usersId);
                ModelState.Remove("Users");

                if (ModelState.IsValid)
                {
                    if (filter == null)
                    {
                        product.UsersId = usersId;
                        _context.Product.Add(product);
                        _context.SaveChanges();

                        TempData["success"] = "Product added successfully.";
                        return RedirectToAction("ProductList");
                    }
                    else
                    {
                        TempData["error"] = "Product already exists.";
                        return View();
                    }
                }
                TempData["error"] = "Fill up all the fields.";
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public IActionResult ProductEdit(int Id)
        {
            try
            {
                var product = _context.ProductVariants
                            .Include(v => v.Product)
                            .Include(v => v.Gallery)
                            .FirstOrDefault(v => v.Id == Id);

                return View(product);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpPost]
        public IActionResult ProductEdit(ProductVariants product, string ButtonType, int Id)
        {
            try
            {
                ModelState.Remove("Product");

                if (ModelState.IsValid)
                {
                    var existing = _context.ProductVariants.FirstOrDefault(p => p.Id == Id);

                    if (existing == null)
                    {
                        TempData["error"] = "Product variant not found.";
                        return RedirectToAction("ProductList");
                    }

                    if (ButtonType == "Save")
                    {
                        existing.ProductId = product.ProductId;
                        existing.Material = product.Material;
                        existing.Dimension = product.Dimension;
                        existing.Color = product.Color;
                        existing.Price = product.Price;
                        existing.Quantity = product.Quantity;
                        existing.Discount = product.Discount;

                        _context.ProductVariants.Update(existing);
                        _context.SaveChanges();

                        TempData["success"] = "Item successfully updated";
                    }
                    else if (ButtonType == "Remove")
                    {
                        existing.isArchive = DateTime.Now;
                        _context.ProductVariants.Update(existing);
                        _context.SaveChanges();

                        TempData["success"] = "Item successfully deleted";
                        return RedirectToAction("ProductList");
                    }
                }

                var productVar = _context.ProductVariants
                            .Include(v => v.Product)
                            .Include(v => v.Gallery)
                            .FirstOrDefault(v => v.Id == Id);

                return View(productVar);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public IActionResult ProductList()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UsersId")?.Value);

                var data = _context.Product
                            .Include(u => u.ProductVariants.Where(g => g.isArchive == null))
                            .Include(g => g.Gallery)
                            .Where(d => d.UsersId == userId)
                            .ToList();

                return View(data);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public IActionResult ProductDetails(int Id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UsersId")?.Value);

                var data = _context.ProductVariants
                        .Include(v => v.Product)
                        .Where(u => u.ProductId == Id && u.Product.UsersId == userId && u.isArchive == null)
                        .ToList();

                var productName = _context.Product
                                  .FirstOrDefault(n => n.UsersId == userId && n.Id == Id);

                var viewModel = new ProductDetailsViewModel
                {
                    Product = productName,
                    ProductVariants = data
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpPost]
        public IActionResult ProductDetails(ProductDetailsViewModel model, IFormFile Image, int Id)
        {
            try
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
                        // New variant
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
                        TempData["success"] = "Item successfully added";
                    }
                    else
                    {
                        // Update existing
                        productVariant = _context.ProductVariants.FirstOrDefault(p => p.Id == model.ProductDetails.Id);

                        if (productVariant == null)
                        {
                            TempData["error"] = "Variant not found.";
                            return RedirectToAction("ProductDetails", new { Id });
                        }

                        productVariant.ProductId = Id;
                        productVariant.Material = model.ProductDetails.Material;
                        productVariant.Dimension = model.ProductDetails.Dimension;
                        productVariant.Color = model.ProductDetails.Color;
                        productVariant.Price = model.ProductDetails.Price;
                        productVariant.Quantity = model.ProductDetails.Quantity;
                        productVariant.Discount = model.ProductDetails.Discount;

                        _context.ProductVariants.Update(productVariant);
                        TempData["success"] = "Item successfully updated";
                    }

                    _context.SaveChanges();

                    // Add image if uploaded
                    if (Image != null)
                    {
                        using var ms = new MemoryStream();
                        Image.CopyTo(ms);

                        var gallery = new Gallery
                        {
                            Image = ms.ToArray(),
                            pUsersId = int.Parse(User.FindFirst("UsersId")?.Value),
                            ProductId = Id,
                            ProductVariantsId = productVariant.Id,
                            ImageType = "Item image"
                        };
                        _context.Gallery.Add(gallery);
                        _context.SaveChanges();
                    }

                    return RedirectToAction("ProductDetails", new { Id });
                }

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public IActionResult Users()
        {
            try { return View(); }
            catch (Exception) { return RedirectToAction("Index", "Error"); }
        }

        public IActionResult Reports()
        {
            try { return View(); }
            catch (Exception) { return RedirectToAction("Index", "Error"); }
        }

        public IActionResult StoreSettings()
        {
            try { return View(); }
            catch (Exception) { return RedirectToAction("Index", "Error"); }
        }

        public IActionResult Orders()
        {
            try { return View(); }
            catch (Exception) { return RedirectToAction("Index", "Error"); }
        }
    }
}
