using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CSI402_Project.Controllers;

public class ProductController : Controller
{
    private readonly BikeShopDbContext _context;

    public ProductController(BikeShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string search, int? categoryId, string priceRange, string sortBy, int page = 1)
    {
        int pageSize = 9;
        
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        // ค้นหาตามชื่อ
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search));
            ViewBag.Search = search;
        }

        // กรองตามหมวดหมู่
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
            var category = await _context.Categories.FindAsync(categoryId.Value);
            ViewBag.CategoryName = category?.Name;
            ViewBag.CategoryId = categoryId.Value;
        }

        // กรองตามช่วงราคา
        if (!string.IsNullOrEmpty(priceRange))
        {
            ViewBag.PriceRange = priceRange;
            switch (priceRange)
            {
                case "under2000":
                    query = query.Where(p => p.Price < 2000);
                    break;
                case "2000-3000":
                    query = query.Where(p => p.Price >= 2000 && p.Price <= 3000);
                    break;
                case "3000-5000":
                    query = query.Where(p => p.Price >= 3000 && p.Price <= 5000);
                    break;
                case "over5000":
                    query = query.Where(p => p.Price > 5000);
                    break;
            }
        }

        // เรียงลำดับ
        sortBy = sortBy ?? "default";
        ViewBag.SortBy = sortBy;
        query = sortBy switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            "newest" => query.OrderByDescending(p => p.ProductId),
            _ => query.OrderBy(p => p.ProductId)
        };

        // นับจำนวนทั้งหมด
        var totalItems = await query.CountAsync();
        ViewBag.TotalItems = totalItems;

        // Pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // ส่งรายการหมวดหมู่ไป View
        ViewBag.Categories = await _context.Categories.ToListAsync();

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }
}