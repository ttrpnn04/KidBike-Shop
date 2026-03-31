using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Project.Controllers;

public class PromotionController : Controller
{
    private readonly BikeShopDbContext _context;

    public PromotionController(BikeShopDbContext context)
    {
        _context = context;
    }

    // GET: /Promotion - แสดงโปรโมชั่นทั้งหมดที่กำลังใช้งานได้
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Now;
        
        var promotions = await _context.Promotions
            .Where(p => p.StartDate <= today && p.EndDate >= today)
            .OrderByDescending(p => p.EndDate)
            .ToListAsync();

        return View(promotions);
    }

    // GET: /Promotion/Details/5 - แสดงรายละเอียดโปรโมชั่น
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var promotion = await _context.Promotions
            .FirstOrDefaultAsync(p => p.PromotionId == id);

        if (promotion == null)
        {
            return NotFound();
        }

        return View(promotion);
    }
}
