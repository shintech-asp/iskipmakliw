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
        public IActionResult Search(string query)
        {
            var products = _context.Product
                .Where(p => p.Name.Contains(query))
                .Select(p => new ClientViewModel
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    SellerName = p.Users.Username,
                    SellerId = p.UsersId,
                    Price = p.ProductVariants
                        .OrderBy(v => v.Price)
                        .Select(v => v.Price)
                        .FirstOrDefault(),
                    Image = p.ProductVariants.FirstOrDefault().ProductImage ?? "/src/assets/img/OakMartLogo.png"
                })
                .ToList();

            var sellers = _context.Users
                .Where(u => u.Username.Contains(query))
                .Select(u => new SellerViewModel
                {
                    SellerId = u.Id,
                    Role = u.Role,
                    SellerName = u.Username,
                    ProfileImage = "/src/assets/img/OakMartLogo.png"
                })
                .Where(u => u.Role != "Admin")
                .ToList();

            ViewBag.Query = query;

            return View(new SearchResultViewModel
            {
                Products = products,
                Sellers = sellers
            });
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
            var conSales = _context.DeliverProduct
                            .Include(u => u.PurchasedProduct)
                                .ThenInclude(u => u.ProductVariants)
                                    .ThenInclude(u => u.Product)
                .Where(u => u.Status == "Delivered" && u.PurchasedProduct.ProductVariants.Product.Users.Id == usersId).ToList();
            var data = new SellersIndexViewModel
            {
                Users = user,
                PurchasedProduct = recent,
                ProductVariants = product,
                Sales = conSales
            };
            return View(data);
        }
        public IActionResult Approve3DOrder(int Id)
        {
            var data = _context.CustomizationOrders.Find(Id);

            data.SellerStatus = "Approved";
            _context.CustomizationOrders.Update(data);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        public IActionResult GetPrice(int Id)
        {
            var data = _context.CustomizationOrders.Where(u => u.Id == Id).FirstOrDefault();

            return Json(new { success = true, Price = data?.Price ?? 0, ModeOfPayment = data?.ModeOfPayment ?? "No selected yet" });

        }


        public IActionResult Add3dPrice(int Id, decimal Price, string ModeOfPayment)
        {
            var data = _context.CustomizationOrders.Find(Id);

            data.Price = Price;
            data.ModeOfPayment = ModeOfPayment;
            _context.CustomizationOrders.Update(data);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        public IActionResult Deliver3d(int Id)
        {
            var data = _context.CustomizationOrders.Find(Id);
            data.TransactionStatus = "Shipping";
            _context.CustomizationOrders.Update(data);
            var purchased3d = _context.PurchasedProduct.Where(u => u.CustomizationOrdersId == Id).FirstOrDefault();
            var updatePurchase = _context.PurchasedProduct.Find(purchased3d.Id);
            updatePurchase.TransactionStatus = "To deliver";
            var deliverProduct = new DeliverProduct
            {
                PurchasedProductId = purchased3d.Id,
                Status = "Pending",
            };
            _context.PurchasedProduct.Update(updatePurchase);
            _context.DeliverProduct.Add(deliverProduct);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        public IActionResult Mark3dAsComplete(int Id)
        {
            var data = _context.CustomizationOrders.Find(Id);
            data.TransactionStatus = "Completed";
            _context.CustomizationOrders.Update(data);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        public IActionResult Cancel3d(int Id)
        {
            var data = _context.CustomizationOrders.Find(Id);
            data.TransactionStatus = "Cancelled";
            _context.CustomizationOrders.Update(data);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult SendMessage(int Id, string Message)
        {
            var message = _context.CustomizationChat.Where(u => u.CustomizationOrdersId == Id).FirstOrDefault();
            var CustomizationChat = new CustomizationChat
            {
                UsersId = int.Parse(User.FindFirst("UsersId").Value),
                SellersId = message.SellersId,
                Message = Message,
                CustomizationOrdersId = message.CustomizationOrdersId,
                IsFromBuyer = false
            };
            _context.CustomizationChat.Add(CustomizationChat);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpGet]
        public IActionResult GetNewMessages(int orderId, int lastMessageId)
        {
            var newMessages = _context.CustomizationChat
                .Where(c => c.CustomizationOrdersId == orderId && c.Id > lastMessageId)
                .OrderBy(c => c.Id)
                .Select(c => new {
                    c.Id,
                    c.Message,
                    c.DateSent,
                    IsCustomer = c.IsFromBuyer
                })
                .ToList();

            return Json(newMessages);
        }

        public IActionResult LoadChatPartial(int orderId)
        {
            var messages = _context.CustomizationChat
                .Where(c => c.CustomizationOrdersId == orderId)
                .Include(c => c.CustomizationOrders)
                .Include(c => c.Sellers)
                .Include(c => c.Users)
                .OrderBy(c => c.DateSent)
                .ToList();

            return PartialView("_ChatConversationPartial", messages);
        }
        public IActionResult Chat()
        {
            var usersId = int.Parse(User.FindFirst("UsersId").Value);
            var data = _context.CustomizationChat
                        .Include(u => u.Users)
                            .ThenInclude(u => u.UserDetails)
                        .Include(u => u.Sellers)
                            .ThenInclude(u => u.UserDetails)
                        .Where(u => u.SellersId == usersId)
                        .ToList();
            return View(data);
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
            ModelState.Remove("ProductDetails.Ratings");
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
                                    .Include(u => u.PurchasedProduct)
                                        .ThenInclude(pp => pp.CustomizationOrders)
                                            .ThenInclude(pp => pp.Sellers)
                                                .ThenInclude(pp => pp.UserDetails)
                                    .Include(u => u.PurchasedProduct)
                                        .ThenInclude(pp => pp.CustomizationOrders)
                                            .ThenInclude(pp => pp.Users)
                                                .ThenInclude(pp => pp.UserDetails)
                                    .Where(u => u.PurchasedProduct.Id == Id)
                                    .FirstOrDefault();
            var Rated = _context.Ratings
                            .Include(u => u.ProductVariants)
                                .ThenInclude(u => u.Product)
                            .ThenInclude(u => u.Users)
                                .ThenInclude(u => u.UserDetails)
                            .Include(u => u.PurchasedProduct)
                                .ThenInclude(u => u.Users)
                                    .ThenInclude(u => u.UserDetails)
                            .Include(u => u.PurchasedProduct)
                                .ThenInclude(pp => pp.CustomizationOrders)
                                    .ThenInclude(pp => pp.Sellers)
                                        .ThenInclude(pp => pp.UserDetails)
                            .Include(u => u.PurchasedProduct)
                                .ThenInclude(pp => pp.CustomizationOrders)
                                    .ThenInclude(pp => pp.Users)
                                        .ThenInclude(pp => pp.UserDetails)
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
                        .Include(pp => pp.CustomizationOrders)
                            .ThenInclude(pp => pp.Sellers)
                                .ThenInclude(pp => pp.UserDetails)
                        .Include(pp => pp.CustomizationOrders)
                            .ThenInclude(pp => pp.Users)
                                .ThenInclude(pp => pp.UserDetails)
                        .Where(u => u.ProductVariants.Product.Users.Id == usersId || u.CustomizationOrders.SellersId == usersId)
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
