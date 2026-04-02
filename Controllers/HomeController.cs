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

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel();

        // ดึงหมวดหมู่สินค้าทั้งหมด
        viewModel.Categories = await _context.Categories
            .Include(c => c.Products)
            .ToListAsync();

        // ดึงสินค้ายอดนิยม (สินค้าที่มีจำนวนสั่งซื้อมากที่สุด 4 รายการ)
        var popularProductIds = await _context.OrderDetails
            .GroupBy(od => od.ProductId)
            .OrderByDescending(g => g.Sum(od => od.Quantity))
            .Select(g => g.Key)
            .Take(4)
            .ToListAsync();

        if (popularProductIds.Any())
        {
            viewModel.PopularProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => popularProductIds.Contains(p.ProductId))
                .ToListAsync();
        }
        else
        {
            // ถ้ายังไม่มีข้อมูลการสั่งซื้อ ให้แสดงสินค้าล่าสุด 4 รายการแทน
            viewModel.PopularProducts = await _context.Products
                .Include(p => p.Category)
                .Take(4)
                .ToListAsync();
        }

        // ดึงโปรโมชั่นที่กำลังใช้งานได้
        viewModel.Promotions = await _context.Promotions
            .Where(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
            .Take(3)
            .ToListAsync();

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
