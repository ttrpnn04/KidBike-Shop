using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using System.Linq;

public class ProductController : Controller
{
    private readonly BikeShopDbContext _context;

    public ProductController(BikeShopDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var products = _context.Products.ToList();
        return View(products);
    }
}