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
            var user = _context.Users
                        .Include(u => u.UserDetails)
                        .Include(u => u.Subscription)
                            .ThenInclude(u => u.Plans)
                        .Include(u => u.Payments)
                        .Include(u => u.Product)
                        .Include(u => u.Billings)
                        .Where(u => u.Id == usersId)
                        .FirstOrDefault();
            var recent = _context.PurchasedProduct
                        .Include(u => u.ProductVariants)
                            .ThenInclude(u => u.Product)
                        .Include(u => u.Users)
                        .Where(u => u.ProductVariants.Product.UsersId == usersId).ToList();
            var product = _context.ProductVariants
                          .Include(u => u.Product)
                          .Include(u => u.PurchasedProduct)
                          .Where(u => u.Product.UsersId == usersId).ToList();
            var data = new SellersIndexViewModel
            {
                Users = user,
                PurchasedProduct = recent,
                ProductVariants = product
            };
            return View(data);
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
            var usersId = int.Parse(User.FindFirst("UsersId")?.Value);
            var filter = _context.Product.Where(u => u.Name == product.Name && u.UsersId == usersId).FirstOrDefault();
            ModelState.Remove("Users");
            if (ModelState.IsValid)
            {
                if (filter == null)
                {
                    product.UsersId = usersId;
                    var newProduct = _context.Product.Add(product);
                   
                    _context.SaveChanges();
                    TempData["Success"] = "Product added successfully.";
                    return View(product);
                }
                else
                {
                    TempData["Error"] = "Product already exists.";
                    return View();
                }
            }
            else
            {
                TempData["Error"] = "Fill up all the fields";
                return View();
            }
                
        }
        public IActionResult ProductEdit(int Id)
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);
            var product = _context.ProductVariants
                        .Include(v => v.Product)
                        .FirstOrDefault(v => v.Id == Id);
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
                        _context.SaveChanges();
                        TempData["Success"] = "Item successfully updated";
                        return RedirectToAction("Index");
                    }
                }else if(ButtonType == "Remove")
                {
                    var existing = _context.ProductVariants.FirstOrDefault(p => p.Id == Id);
                    if (existing != null)
                    {
                        existing.isArchive = DateTime.Now;
                        _context.ProductVariants.Update(existing);
                        _context.SaveChanges();
                        TempData["Success"] = "Item successfully deleted";
                        return RedirectToAction("Index");
                    }

                }
            }
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);

            var productVar = _context.ProductVariants
                        .Include(v => v.Product)
                        .FirstOrDefault(v => v.Id == Id);
            return View(productVar);
        }
        public IActionResult ProductList()
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);

            var data = _context.Product
                        .Include(u => u.ProductVariants.Where(g => g.isArchive == null))
                        .Where(d => d.UsersId == userId)
                        .ToList();

            return View(data);
        }
        public IActionResult ProductDetails(int Id)
        {
            var userId = int.Parse(User.FindFirst("UsersId")?.Value);

            var data = _context.ProductVariants
                    .Include(v => v.Product)
                    .Where(u => u.ProductId == Id && u.Product.UsersId == userId && u.isArchive == null)
                    .ToList();

            var productName = _context.Product
                              .Where(n => n.UsersId == userId && n.Id == Id)
                              .FirstOrDefault();

            var ProductDetailsViewModel = new ProductDetailsViewModel
            {
                Product = productName,
                ProductVariants = data
            };
            return View(ProductDetailsViewModel);
        }
        [HttpPost]
        public IActionResult ProductDetails(ProductDetailsViewModel model, IFormFile ProductVariantImage, int Id)
        {
            if (model?.ProductDetails == null)
                return View(model);

            ModelState.Remove("Product");
            ModelState.Remove("ProductVariants");
            ModelState.Remove("ProductDetails.Product");
            ModelState.Remove("ProductDetails.ProductId");
            ModelState.Remove("ProductDetails.Carts");
            ModelState.Remove("ProductDetails.PurchasedProduct");
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
                    productVariant = _context.ProductVariants.FirstOrDefault(v => v.Id == model.ProductDetails.Id);
                    if (productVariant == null) return NotFound();

                    productVariant.Material = model.ProductDetails.Material;
                    productVariant.Dimension = model.ProductDetails.Dimension;
                    productVariant.Color = model.ProductDetails.Color;
                    productVariant.Price = model.ProductDetails.Price;
                    productVariant.Quantity = model.ProductDetails.Quantity;
                    productVariant.Discount = model.ProductDetails.Discount;
                }

                _context.SaveChanges(); // save here so productVariant.Id is available

                // ✅ Save uploaded image as file path
                if (ProductVariantImage != null && ProductVariantImage.Length > 0)
                {
                    var fileName = $"product_{Guid.NewGuid()}{Path.GetExtension(ProductVariantImage.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        ProductVariantImage.CopyTo(stream);
                    }

                    // Save relative path to DB
                    productVariant.ProductImage = $"/uploads/{fileName}";

                    _context.ProductVariants.Update(productVariant);
                    _context.SaveChanges();
                }

                TempData["Success"] = "Item successfully added";
                return RedirectToAction("Index", "Seller");
            }
            else
            {
                TempData["Error"] = "Fill up all the fields";
                return RedirectToAction("ProductDetails");
            }

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
        public IActionResult OrderDetails(int Id, int ProductId)
        {
            var toDeliver = _context.DeliverProduct
                                    .Include(u => u.PurchasedProduct)
                                        .ThenInclude(u => u.ProductVariants)
                                            .ThenInclude(u => u.Product)
                                                .ThenInclude(u => u.Users)
                                                    .ThenInclude(u => u.UserDetails)
                                    .Include(u => u.PurchasedProduct)
                                        .ThenInclude(u => u.Users)
                                            .ThenInclude(u => u.UserDetails)
                                    .Where(u => u.PurchasedProduct.Id == Id)
                                    .FirstOrDefault();
            var Rated = _context.Ratings
                            .Include(u => u.ProductVariants)
                                .ThenInclude(u => u.Product)
                            .ThenInclude(u => u.Users)
                                .ThenInclude(u => u.UserDetails)
                            .Where(u => u.PurchasedProductId == Id)
                            .FirstOrDefault();

            var timeline = new TimelineViewModel
            {
                DeliverProduct = toDeliver,
                Ratings = Rated
            };
            return View(timeline);
        }
        public IActionResult Deliver(int Id)
        {
            var data = _context.PurchasedProduct.Find(Id);
            if(data != null)
            {
                var deliver = new DeliverProduct
                {
                    PurchasedProductId = data.Id,
                    Status = "Pending"
                };
                _context.DeliverProduct.Add(deliver);
                data.TransactionStatus = "To deliver";
                _context.PurchasedProduct.Update(data);
                _context.SaveChanges();
            }
            return Json(new { success = true });
        }
        public IActionResult Orders()
        {
            int usersId = int.Parse(User.FindFirst("UsersId")?.Value);
            var data = _context.PurchasedProduct
                        .Include(u => u.ProductVariants)
                            .ThenInclude(u => u.Product)
                                .ThenInclude(u => u.Users)
                                    .ThenInclude(u => u.UserDetails)
                        .Include(u => u.Users)
                            .ThenInclude(u => u.UserDetails)
                        .Where(u => u.ProductVariants.Product.Users.Id == usersId)
                        .ToList();
            var toDeliver = _context.DeliverProduct
                                    .Include(u => u.PurchasedProduct)
                                        .ThenInclude(u => u.ProductVariants)
                                            .ThenInclude(u => u.Product)
                                                .ThenInclude(u => u.Users)
                                                    .ThenInclude(u => u.UserDetails)
                                    .Include(u => u.PurchasedProduct)
                                        .ThenInclude(u => u.Users)
                                            .ThenInclude(u => u.UserDetails)
                                    .Where(u => u.PurchasedProduct.ProductVariants.Product.Users.Id == usersId)
                                    .ToList();
            var order = new SellerOrderViewModel
            {
                PurchasedProduct = data,
                DeliverProduct = toDeliver
            };
            return View(order);
        }
    }
}
