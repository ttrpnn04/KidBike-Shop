using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Project.Controllers;

public class AddressController : Controller
{
    private readonly BikeShopDbContext _context;

    public AddressController(BikeShopDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนเข้าถึงข้อมูลที่อยู่";
            return RedirectToAction("Login", "Account");
        }

        var user = _context.Users.Find(userId);
        
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    public IActionResult Update(string address, string phone, string email)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = _context.Users.Find(userId);
        
        if (user == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(address))
            user.Address = address;
        if (!string.IsNullOrEmpty(phone))
            user.Phone = phone;
        if (!string.IsNullOrEmpty(email))
            user.Email = email;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "อัปเดตข้อมูลที่อยู่จัดส่งเรียบร้อยแล้ว";
        return RedirectToAction("Index");
    }
}
