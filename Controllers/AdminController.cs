using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using CSI402_Project.Filters;
using Microsoft.EntityFrameworkCore;
using CSI402_Project.ViewModels;

namespace CSI402_Project.Controllers;

[AdminAuthorize]
public class AdminController : Controller
{
        private readonly BikeShopDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(BikeShopDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Dashboard()
        {
            var dashboardData = new DashboardViewModel
            {
                TotalProducts = _context.Products.Count(),
                TotalCategories = _context.Categories.Count(),
                TotalOrders = _context.Orders.Count(),
                TotalUsers = _context.Users.Count(),
                TotalPromotions = _context.Promotions.Count(),
                RecentOrders = _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToList(),
                LowStockProducts = _context.Products
                    .Where(p => p.Stock < 10)
                    .OrderBy(p => p.Stock)
                    .Take(5)
                    .ToList()
            };

            return View(dashboardData);
        }

        public IActionResult Products()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ToList();
            return View(products);
        }

        public IActionResult CreateProduct()
        {
            var viewModel = new ProductFormViewModel
            {
                Categories = _context.Categories.ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(Product product, 
            IFormFile MainImageFile, List<IFormFile> AdditionalImageFiles, List<string> AdditionalImageUrls)
        {
            if (ModelState.IsValid)
            {
                if (MainImageFile != null && MainImageFile.Length > 0)
                {
                    product.ImageUrl = SaveImageFile(MainImageFile);
                }

                _context.Products.Add(product);
                _context.SaveChanges();

                int order = 1;
                if (AdditionalImageFiles != null)
                {
                    foreach (var file in AdditionalImageFiles.Where(f => f != null && f.Length > 0))
                    {
                        var imageUrl = SaveImageFile(file);
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = imageUrl,
                            DisplayOrder = order++
                        });
                    }
                }

                if (AdditionalImageUrls != null)
                {
                    foreach (var imageUrl in AdditionalImageUrls.Where(url => !string.IsNullOrWhiteSpace(url)))
                    {
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = imageUrl,
                            DisplayOrder = order++
                        });
                    }
                }

                if ((AdditionalImageFiles != null && AdditionalImageFiles.Any(f => f != null && f.Length > 0)) ||
                    (AdditionalImageUrls != null && AdditionalImageUrls.Any(url => !string.IsNullOrWhiteSpace(url))))
                {
                    _context.SaveChanges();
                }

                TempData["SuccessMessage"] = "เพิ่มสินค้าสำเร็จ";
                return RedirectToAction(nameof(Products));
            }
            var viewModel = new ProductFormViewModel
            {
                Product = product,
                Categories = _context.Categories.ToList()
            };
            return View(viewModel);
        }

        private string SaveImageFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return $"/images/products/{uniqueFileName}";
        }

        public IActionResult EditProduct(int id)
        {
            var product = _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            var viewModel = new ProductFormViewModel
            {
                Product = product,
                Categories = _context.Categories.ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(int id, Product product, 
            List<int> ExistingImageIds, List<string> ExistingImageUrls, List<string> AdditionalImageUrls)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);

                    var currentImages = _context.ProductImages
                        .Where(pi => pi.ProductId == id)
                        .ToList();

                    var submittedExistingIds = ExistingImageIds ?? new List<int>();
                    var imagesToDelete = currentImages.Where(ci => !submittedExistingIds.Contains(ci.ProductImageId)).ToList();
                    _context.ProductImages.RemoveRange(imagesToDelete);

                    if (ExistingImageIds != null && ExistingImageUrls != null)
                    {
                        for (int i = 0; i < ExistingImageIds.Count; i++)
                        {
                            var imageId = ExistingImageIds[i];
                            var imageUrl = i < ExistingImageUrls.Count ? ExistingImageUrls[i] : null;
                            
                            if (!string.IsNullOrWhiteSpace(imageUrl))
                            {
                                var existingImage = currentImages.FirstOrDefault(ci => ci.ProductImageId == imageId);
                                if (existingImage != null)
                                {
                                    existingImage.ImageUrl = imageUrl;
                                    existingImage.DisplayOrder = i + 1;
                                }
                            }
                        }
                    }

                    if (AdditionalImageUrls != null && AdditionalImageUrls.Any())
                    {
                        int startOrder = (ExistingImageUrls?.Count ?? 0) + 1;
                        int order = startOrder;
                        foreach (var imageUrl in AdditionalImageUrls.Where(url => !string.IsNullOrWhiteSpace(url)))
                        {
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.ProductId,
                                ImageUrl = imageUrl,
                                DisplayOrder = order++
                            });
                        }
                    }

                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "แก้ไขสินค้าสำเร็จ";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Products));
            }
            var viewModel = new ProductFormViewModel
            {
                Product = product,
                Categories = _context.Categories.ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "ลบสินค้าสำเร็จ";
            return RedirectToAction(nameof(Products));
        }

        public IActionResult Categories()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "เพิ่มหมวดหมู่สำเร็จ";
                return RedirectToAction(nameof(Categories));
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Update(category);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "แก้ไขหมวดหมู่สำเร็จ";
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            var hasProducts = _context.Products.Any(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["ErrorMessage"] = "ไม่สามารถลบหมวดหมู่ที่มีสินค้าอยู่ได้";
                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "ลบหมวดหมู่สำเร็จ";
            return RedirectToAction(nameof(Categories));
        }

        public IActionResult Promotions()
        {
            var promotions = _context.Promotions.ToList();
            return View(promotions);
        }

        public IActionResult CreatePromotion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePromotion(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                _context.Promotions.Add(promotion);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "เพิ่มโปรโมชั่นสำเร็จ";
                return RedirectToAction(nameof(Promotions));
            }
            return RedirectToAction(nameof(Promotions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPromotion(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                _context.Update(promotion);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "แก้ไขโปรโมชั่นสำเร็จ";
            }
            return RedirectToAction(nameof(Promotions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePromotion(int id)
        {
            var promotion = _context.Promotions.Find(id);
            if (promotion == null)
            {
                return NotFound();
            }

            _context.Promotions.Remove(promotion);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "ลบโปรโมชั่นสำเร็จ";
            return RedirectToAction(nameof(Promotions));
        }

        public IActionResult Orders()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null)
            {
                return NotFound();
            }

            var validStatuses = new[] { "Pending", "Paid", "Shipping", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                TempData["ErrorMessage"] = "สถานะไม่ถูกต้อง";
                return RedirectToAction(nameof(Orders));
            }

            order.Status = status;
            _context.SaveChanges();
            TempData["SuccessMessage"] = "อัปเดตสถานะคำสั่งซื้อสำเร็จ";
            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
}

