using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Project.Controllers;

public class CartController : Controller
{
    private readonly BikeShopDbContext _context;

    public CartController(BikeShopDbContext context)
    {
        _context = context;
    }

    // GET: /Cart - แสดงตะกร้าสินค้า
    public IActionResult Index()
    {
        // TODO: รับ UserId จาก Session หรือ Claims จริงๆ
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            // ถ้ายังไม่ล็อกอิน แสดงตะกร้าว่าง
            return View(new List<Cart>());
        }

        var cartItems = _context.Carts
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToList();

        return View(cartItems);
    }

    // POST: /Cart/Add - เพิ่มสินค้าลงตะกร้า
    [HttpPost]
    public IActionResult Add(int productId, int quantity = 1)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนเพิ่มสินค้าลงตะกร้า";
            return RedirectToAction("Login", "Account");
        }

        // ตรวจสอบว่ามีสินค้านี้ในตะกร้าอยู่แล้วหรือไม่
        var existingItem = _context.Carts
            .FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

        if (existingItem != null)
        {
            // ถ้ามีอยู่แล้ว เพิ่มจำนวน
            existingItem.Quantity += quantity;
        }
        else
        {
            // ถ้ายังไม่มี เพิ่มรายการใหม่
            var cartItem = new Cart
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity
            };
            _context.Carts.Add(cartItem);
        }

        _context.SaveChanges();
        TempData["SuccessMessage"] = "เพิ่มสินค้าลงตะกร้าเรียบร้อยแล้ว";
        
        return RedirectToAction("Index");
    }

    // POST: /Cart/Update - อัพเดทจำนวนสินค้า
    [HttpPost]
    public IActionResult Update(int cartId, int quantity)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItem = _context.Carts
            .FirstOrDefault(c => c.CartId == cartId && c.UserId == userId);

        if (cartItem == null)
        {
            return NotFound();
        }

        if (quantity <= 0)
        {
            _context.Carts.Remove(cartItem);
        }
        else
        {
            cartItem.Quantity = quantity;
        }

        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    // POST: /Cart/Remove - ลบสินค้าออกจากตะกร้า
    [HttpPost]
    public IActionResult Remove(int cartId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItem = _context.Carts
            .FirstOrDefault(c => c.CartId == cartId && c.UserId == userId);

        if (cartItem != null)
        {
            _context.Carts.Remove(cartItem);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "ลบสินค้าออกจากตะกร้าเรียบร้อยแล้ว";
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/Clear - ล้างตะกร้าทั้งหมด
    [HttpPost]
    public IActionResult Clear()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = _context.Carts
            .Where(c => c.UserId == userId)
            .ToList();

        _context.Carts.RemoveRange(cartItems);
        _context.SaveChanges();
        
        TempData["SuccessMessage"] = "ล้างตะกร้าสินค้าเรียบร้อยแล้ว";
        return RedirectToAction("Index");
    }
}
