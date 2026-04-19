using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using Microsoft.EntityFrameworkCore;
using CSI402_Project.ViewModels;

namespace CSI402_Project.Controllers;

public class OrderController : Controller
{
    private readonly BikeShopDbContext _context;

    public OrderController(BikeShopDbContext context)
    {
        _context = context;
    }

    // GET: /Order - แสดงคำสั่งซื้อทั้งหมดของผู้ใช้
    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนเข้าถึงคำสั่งซื้อ";
            return RedirectToAction("Login", "Account");
        }

        var orders = _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View(orders);
    }

    // GET: /Order/Details/5 - แสดงรายละเอียดคำสั่งซื้อ
    public IActionResult Details(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var order = _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefault(o => o.OrderId == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // GET: /Order/Checkout - หน้าดำเนินการสั่งซื้อ
    public IActionResult Checkout()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนดำเนินการสั่งซื้อ";
            return RedirectToAction("Login", "Account");
        }

        var cartItems = _context.Carts
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToList();

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "ตะกร้าสินค้าว่างเปล่า";
            return RedirectToAction("Index", "Cart");
        }

        var user = _context.Users.Find(userId);

        var viewModel = new CheckoutViewModel
        {
            CartItems = cartItems,
            User = user ?? new User()
        };

        // Check for promo code in TempData
        if (TempData["AppliedPromotionId"] != null)
        {
            var promoId = TempData["AppliedPromotionId"] as int? ?? 0;
            if (promoId > 0)
            {
                var promotion = _context.Promotions.Find(promoId);
                if (promotion != null)
                {
                    viewModel.AppliedPromotion = promotion;
                    viewModel.AppliedPromotionId = promoId;
                    viewModel.DiscountAmount = CalculateDiscount(cartItems.Sum(c => (c.Product?.Price ?? 0) * (c.Quantity ?? 0)), promotion);
                    viewModel.PromoCode = promotion.Name;

                    // Keep TempData for next request (POST checkout)
                    TempData.Keep("AppliedPromotionId");
                }
            }
        }

        return View(viewModel);
    }

    // POST: /Order/ApplyPromo - ใช้โค้ดส่วนลด
    [HttpPost]
    public IActionResult ApplyPromo(string promoCode)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = _context.Carts
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToList();

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "ตะกร้าสินค้าว่างเปล่า";
            return RedirectToAction("Index", "Cart");
        }

        var subtotal = cartItems.Sum(c => (c.Product?.Price ?? 0) * (c.Quantity ?? 0));
        var today = DateTime.Now;

        // Find valid promotion by name/code
        var promotion = _context.Promotions
            .FirstOrDefault(p => p.Name == promoCode && 
                                     p.StartDate <= today && 
                                     p.EndDate >= today);

        if (promotion == null)
        {
            TempData["ErrorMessage"] = "โค้ดส่วนลดไม่ถูกต้องหรือหมดอายุแล้ว";
            return RedirectToAction("Checkout");
        }

        // Check condition type (FirstPurchase, MinAmount, etc.)
        var conditionType = promotion.ConditionType?.ToLowerInvariant() ?? "";

        // Check first purchase requirement
        if (conditionType == "firstpurchase")
        {
            var hasOrders = _context.Orders.Any(o => o.UserId == userId);
            if (hasOrders)
            {
                TempData["ErrorMessage"] = "โค้ดนี้ใช้ได้เฉพาะสมาชิกใหม่ (ยังไม่เคยสั่งซื้อ) เท่านั้น";
                return RedirectToAction("Checkout");
            }
        }

        // Check minimum purchase requirement
        if (promotion.ConditionAmount.HasValue && subtotal < promotion.ConditionAmount.Value)
        {
            TempData["ErrorMessage"] = $"ต้องซื้อครบ ฿{promotion.ConditionAmount.Value:N0} จึงจะใช้โค้ดนี้ได้";
            return RedirectToAction("Checkout");
        }

        var discount = CalculateDiscount(subtotal, promotion);
        var newTotal = subtotal - discount;

        TempData["AppliedPromotionId"] = promotion.PromotionId;
        TempData["SuccessMessage"] = $"ใช้โค้ดส่วนลด '{promoCode}' สำเร็จ! ประหยัด ฿{discount:N0}";

        return RedirectToAction("Checkout");
    }

    // Helper method to calculate discount
    private decimal CalculateDiscount(decimal subtotal, Promotion promotion)
    {
        var type = promotion.Type?.ToLowerInvariant() ?? "";
        var discountValue = promotion.DiscountValue ?? 0;

        // Debug: ถ้าส่วนลดเป็น 0 ให้ดูว่า Type เป็นอะไร
        if (discountValue == 0)
        {
            TempData["ErrorMessage"] = $"โปรโมชั่นไม่มีค่าส่วนลด (Type: {promotion.Type}, Value: {promotion.DiscountValue})";
        }

        if (type == "percentage" || type == "percent")
        {
            return subtotal * discountValue / 100;
        }
        else if (type == "fixedamount" || type == "fixed" || type == "amount")
        {
            return Math.Min(discountValue, subtotal);
        }
        else if (type == "freeshipping")
        {
            return 0; // Free shipping handled separately
        }

        // Default: ถ้าไม่รู้จัก Type ให้ใช้ FixedAmount
        return Math.Min(discountValue, subtotal);
    }

    // POST: /Order/RemovePromo - ยกเลิกโค้ดส่วนลด
    [HttpPost]
    public IActionResult RemovePromo()
    {
        TempData.Remove("AppliedPromotionId");
        TempData["SuccessMessage"] = "ยกเลิกโค้ดส่วนลดแล้ว";
        return RedirectToAction("Checkout");
    }

    // POST: /Order/Checkout - ยืนยันการสั่งซื้อ
    [HttpPost]
    public IActionResult Checkout(string shippingAddress, string phone, int? promotionId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = _context.Carts
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToList();

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "ตะกร้าสินค้าว่างเปล่า";
            return RedirectToAction("Index", "Cart");
        }

        // Calculate totals with promotion if applied
        var subtotal = cartItems.Sum(c => (c.Product?.Price ?? 0) * (c.Quantity ?? 0));
        var discount = 0m;
        
        if (promotionId.HasValue)
        {
            var promotion = _context.Promotions.Find(promotionId.Value);
            if (promotion != null && promotion.StartDate <= DateTime.Now && promotion.EndDate >= DateTime.Now)
            {
                discount = CalculateDiscount(subtotal, promotion);
            }
        }

        // สร้างคำสั่งซื้อใหม่
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = "รอดำเนินการ",
            TotalAmount = subtotal - discount,
            PromotionId = promotionId
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

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
        var user = _context.Users.Find(userId);
        if (user != null)
        {
            if (!string.IsNullOrEmpty(shippingAddress))
                user.Address = shippingAddress;
            if (!string.IsNullOrEmpty(phone))
                user.Phone = phone;
        }

        _context.SaveChanges();

        TempData["SuccessMessage"] = discount > 0 
            ? $"สั่งซื้อสำเร็จ! หมายเลขคำสั่งซื้อ #{order.OrderId} (ประหยัด ฿{discount:N0})"
            : $"สั่งซื้อสำเร็จ! หมายเลขคำสั่งซื้อ #{order.OrderId}";
        
        TempData.Remove("AppliedPromotionId");
        return RedirectToAction("Details", new { id = order.OrderId });
    }
}
