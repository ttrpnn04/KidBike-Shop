using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Project.Controllers;

public class OrderController : Controller
{
    private readonly BikeShopDbContext _context;

    public OrderController(BikeShopDbContext context)
    {
        _context = context;
    }

    // GET: /Order - แสดงคำสั่งซื้อทั้งหมดของผู้ใช้
    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนเข้าถึงคำสั่งซื้อ";
            return RedirectToAction("Login", "Account");
        }

        var orders = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    // GET: /Order/Details/5 - แสดงรายละเอียดคำสั่งซื้อ
    public async Task<IActionResult> Details(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // GET: /Order/Checkout - หน้าดำเนินการสั่งซื้อ
    public async Task<IActionResult> Checkout()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนดำเนินการสั่งซื้อ";
            return RedirectToAction("Login", "Account");
        }

        var cartItems = await _context.Carts
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "ตะกร้าสินค้าว่างเปล่า";
            return RedirectToAction("Index", "Cart");
        }

        var user = await _context.Users.FindAsync(userId);
        ViewBag.User = user;

        return View(cartItems);
    }

    // POST: /Order/Checkout - ยืนยันการสั่งซื้อ
    [HttpPost]
    public async Task<IActionResult> Checkout(string shippingAddress, string phone)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = await _context.Carts
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "ตะกร้าสินค้าว่างเปล่า";
            return RedirectToAction("Index", "Cart");
        }

        // สร้างคำสั่งซื้อใหม่
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = "รอดำเนินการ",
            TotalAmount = cartItems.Sum(c => (c.Product?.Price ?? 0) * (c.Quantity ?? 0))
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // สร้างรายละเอียดคำสั่งซื้อ
        foreach (var item in cartItems)
        {
            var orderDetail = new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Product?.Price
            };
            _context.OrderDetails.Add(orderDetail);
        }

        // ลบสินค้าออกจากตะกร้า
        _context.Carts.RemoveRange(cartItems);
        
        // อัปเดตที่อยู่และเบอร์โทรของผู้ใช้
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            if (!string.IsNullOrEmpty(shippingAddress))
                user.Address = shippingAddress;
            if (!string.IsNullOrEmpty(phone))
                user.Phone = phone;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "สั่งซื้อสำเร็จ! หมายเลขคำสั่งซื้อ #" + order.OrderId;
        return RedirectToAction("Details", new { id = order.OrderId });
    }
}
