using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using CSI402_Project.Filters;
using Microsoft.EntityFrameworkCore;
using CSI402_Project.ViewModels;

namespace CSI402_Project.Controllers
{
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

        // GET: /Admin/Dashboard
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

        // ==================== PRODUCT MANAGEMENT ====================

        // GET: /Admin/Products
        public IActionResult Products()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ToList();
            return View(products);
        }

        // GET: /Admin/CreateProduct
        public IActionResult CreateProduct()
        {
            var viewModel = new ProductFormViewModel
            {
                Categories = _context.Categories.ToList()
            };
            return View(viewModel);
        }

        // POST: /Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, 
            IFormFile MainImageFile, List<IFormFile> AdditionalImageFiles, List<string> AdditionalImageUrls)
        {
            if (ModelState.IsValid)
            {
                // Handle main image file upload
                if (MainImageFile != null && MainImageFile.Length > 0)
                {
                    product.ImageUrl = await SaveImageFile(MainImageFile);
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Handle additional image files
                int order = 1;
                if (AdditionalImageFiles != null)
                {
                    foreach (var file in AdditionalImageFiles.Where(f => f != null && f.Length > 0))
                    {
                        var imageUrl = await SaveImageFile(file);
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = imageUrl,
                            DisplayOrder = order++
                        });
                    }
                }

                // Handle additional image URLs
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
                    await _context.SaveChangesAsync();
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

        private async Task<string> SaveImageFile(IFormFile file)
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
                await file.CopyToAsync(fileStream);
            }

            return $"/images/products/{uniqueFileName}";
        }

        // GET: /Admin/EditProduct/5
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

        // POST: /Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, 
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

                    // Get current images from database
                    var currentImages = await _context.ProductImages
                        .Where(pi => pi.ProductId == id)
                        .ToListAsync();

                    // Delete images that are not in the submitted list
                    var submittedExistingIds = ExistingImageIds ?? new List<int>();
                    var imagesToDelete = currentImages.Where(ci => !submittedExistingIds.Contains(ci.ProductImageId)).ToList();
                    _context.ProductImages.RemoveRange(imagesToDelete);

                    // Update existing images
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

                    // Add new images
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

                    await _context.SaveChangesAsync();
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

        // POST: /Admin/DeleteProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "ลบสินค้าสำเร็จ";
            return RedirectToAction(nameof(Products));
        }

        // ==================== CATEGORY MANAGEMENT ====================

        // GET: /Admin/Categories
        public IActionResult Categories()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // GET: /Admin/CreateCategory
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "เพิ่มหมวดหมู่สำเร็จ";
                return RedirectToAction(nameof(Categories));
            }
            return RedirectToAction(nameof(Categories));
        }

        // POST: /Admin/EditCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "แก้ไขหมวดหมู่สำเร็จ";
            }
            return RedirectToAction(nameof(Categories));
        }

        // POST: /Admin/DeleteCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Check if category has products
            var hasProducts = _context.Products.Any(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["ErrorMessage"] = "ไม่สามารถลบหมวดหมู่ที่มีสินค้าอยู่ได้";
                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "ลบหมวดหมู่สำเร็จ";
            return RedirectToAction(nameof(Categories));
        }

        // ==================== PROMOTION MANAGEMENT ====================

        // GET: /Admin/Promotions
        public IActionResult Promotions()
        {
            var promotions = _context.Promotions.ToList();
            return View(promotions);
        }

        // GET: /Admin/CreatePromotion
        public IActionResult CreatePromotion()
        {
            return View();
        }

        // POST: /Admin/CreatePromotion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePromotion(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "เพิ่มโปรโมชั่นสำเร็จ";
                return RedirectToAction(nameof(Promotions));
            }
            return RedirectToAction(nameof(Promotions));
        }

        // POST: /Admin/EditPromotion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPromotion(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                _context.Update(promotion);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "แก้ไขโปรโมชั่นสำเร็จ";
            }
            return RedirectToAction(nameof(Promotions));
        }

        // POST: /Admin/DeletePromotion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "ลบโปรโมชั่นสำเร็จ";
            return RedirectToAction(nameof(Promotions));
        }

        // ==================== ORDER MANAGEMENT ====================

        // GET: /Admin/Orders
        public IActionResult Orders()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        // GET: /Admin/OrderDetails/5
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

        // POST: /Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
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
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "อัปเดตสถานะคำสั่งซื้อสำเร็จ";
            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }

    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPromotions { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
    }
}
