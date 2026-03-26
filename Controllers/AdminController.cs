using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using CSI402_Project.Filters;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Project.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly BikeShopDbContext _context;

        public AdminController(BikeShopDbContext context)
        {
            _context = context;
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
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: /Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "เพิ่มสินค้าสำเร็จ";
                return RedirectToAction(nameof(Products));
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // GET: /Admin/EditProduct/5
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // POST: /Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product)
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
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
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
