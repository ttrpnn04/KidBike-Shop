using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using CSI402_Project.ViewModels;

namespace CSI402_Project.Controllers;

public class ProductController : Controller
{
    private readonly BikeShopDbContext _context;

    public ProductController(BikeShopDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(string search, int? categoryId, string priceRange, string sortBy, int page = 1)
    {
        int pageSize = 9;
        
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        string? categoryName = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || 
                                    (p.Description != null && p.Description.Contains(search)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
            var category = _context.Categories.Find(categoryId.Value);
            categoryName = category?.Name;
        }

        if (!string.IsNullOrEmpty(priceRange))
        {
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

        sortBy = sortBy ?? "default";
        query = sortBy switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            "newest" => query.OrderByDescending(p => p.ProductId),
            _ => query.OrderBy(p => p.ProductId)
        };

        var totalItems = query.Count();

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var viewModel = new ProductIndexViewModel
        {
            Products = items,
            Search = search,
            CategoryId = categoryId,
            PriceRange = priceRange,
            SortBy = sortBy,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            TotalItems = totalItems,
            Categories = _context.Categories.ToList(),
            CategoryName = categoryName
        };

        return View(viewModel);
    }

    public IActionResult Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
            .FirstOrDefault(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }
}