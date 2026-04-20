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

    public IActionResult Index()
    {
        var today = DateTime.Now;
        
        var promotions = _context.Promotions
            .Where(p => p.StartDate <= today && p.EndDate >= today)
            .OrderByDescending(p => p.EndDate)
            .ToList();

        return View(promotions);
    }

    public IActionResult Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var promotion = _context.Promotions
            .FirstOrDefault(p => p.PromotionId == id);

        if (promotion == null)
        {
            return NotFound();
        }

        return View(promotion);
    }
}
