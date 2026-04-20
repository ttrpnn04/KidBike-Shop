using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models;
using CSI402_Project.Models.Db;
using CSI402_Project.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Project.Controllers;

public class HomeController : Controller
{
    private readonly BikeShopDbContext _context;

    public HomeController(BikeShopDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var viewModel = new HomeViewModel();

        viewModel.Categories = _context.Categories
            .Include(c => c.Products)
            .ToList();

        var popularProductIds = _context.OrderDetails
            .GroupBy(od => od.ProductId)
            .OrderByDescending(g => g.Sum(od => od.Quantity))
            .Select(g => g.Key)
            .Take(4)
            .ToList();

        if (popularProductIds.Any())
        {
            viewModel.PopularProducts = _context.Products
                .Include(p => p.Category)
                .Where(p => popularProductIds.Contains(p.ProductId))
                .ToList();
        }
        else
        {
            viewModel.PopularProducts = _context.Products
                .Include(p => p.Category)
                .Take(4)
                .ToList();
        }

        viewModel.Promotions = _context.Promotions
            .Where(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
            .Take(3)
            .ToList();

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Terms()
    {
        return View();
    }

    public IActionResult SizeGuide()
    {
        return View();
    }

    public IActionResult Shipping()
    {
        return View();
    }

    public IActionResult Returns()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
