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
    public IActionResult Index()
    {
        var categories = _context.Categories
            .Include(c => c.Products)
            .ToList();
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
    public IActionResult Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = _context.Categories
            .Include(c => c.Products)
            .FirstOrDefault(c => c.CategoryId == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }
}
