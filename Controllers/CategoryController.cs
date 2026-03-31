using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Project.Controllers;

public class CategoryController : Controller
{
    private readonly BikeShopDbContext _context;

    public CategoryController(BikeShopDbContext context)
    {
        _context = context;
    }

    // GET: /Category - แสดงรายการหมวดหมู่ทั้งหมด
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .ToListAsync();
        return View(categories);
    }

    // GET: /Category/Products/5 - แสดงสินค้าตามหมวดหมู่ (Redirect ไปหน้า Product/Index)
    public IActionResult Products(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Index", "Product");
        }

        return RedirectToAction("Index", "Product", new { categoryId = id });
    }

    // GET: /Category/Details/5 - แสดงรายละเอียดหมวดหมู่
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }
}
