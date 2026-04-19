using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using CSI402_Project.Filters;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Project.Controllers
{
    [SuperAdminAuthorize]
    public class SuperAdminController : Controller
    {
        private readonly BikeShopDbContext _context;

        public SuperAdminController(BikeShopDbContext context)
        {
            _context = context;
        }

        // GET: /SuperAdmin/Users
        public IActionResult Users()
        {
            var users = _context.Users
                .OrderByDescending(u => u.Role == "SuperAdmin")
                .ThenByDescending(u => u.Role == "Admin")
                .ThenBy(u => u.Username)
                .ToList();
            return View(users);
        }

        // POST: /SuperAdmin/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignRole(int userId, string role)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent changing own role (SuperAdmin cannot demote themselves)
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == userId && role != "SuperAdmin")
            {
                TempData["ErrorMessage"] = "ไม่สามารถเปลี่ยนบทบาทตัวเองได้";
                return RedirectToAction(nameof(Users));
            }

            // Validate role
            var validRoles = new[] { "User", "Admin", "SuperAdmin" };
            if (!validRoles.Contains(role))
            {
                TempData["ErrorMessage"] = "บทบาทไม่ถูกต้อง";
                return RedirectToAction(nameof(Users));
            }

            user.Role = role;
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"เปลี่ยนบทบาทผู้ใช้ {user.Username} เป็น {role} สำเร็จ";
            return RedirectToAction(nameof(Users));
        }

        // POST: /SuperAdmin/DeleteUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting own account
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "ไม่สามารถลบบัญชีตัวเองได้";
                return RedirectToAction(nameof(Users));
            }

            // Check if user has orders
            var hasOrders = _context.Orders.Any(o => o.UserId == id);
            if (hasOrders)
            {
                TempData["ErrorMessage"] = "ไม่สามารถลบผู้ใช้ที่มีคำสั่งซื้อได้";
                return RedirectToAction(nameof(Users));
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "ลบผู้ใช้สำเร็จ";
            return RedirectToAction(nameof(Users));
        }

        // POST: /SuperAdmin/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAdmin(string username, string email, string password, string phone)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "กรุณากรอกข้อมูลให้ครบถ้วน";
                return RedirectToAction(nameof(Users));
            }

            // Check if username exists
            if (_context.Users.Any(u => u.Username == username))
            {
                TempData["ErrorMessage"] = "ชื่อผู้ใช้นี้ถูกใช้งานแล้ว";
                return RedirectToAction(nameof(Users));
            }

            // Check if email exists
            if (_context.Users.Any(u => u.Email == email))
            {
                TempData["ErrorMessage"] = "อีเมลนี้ถูกใช้งานแล้ว";
                return RedirectToAction(nameof(Users));
            }

            // Hash password
            var hashedPassword = HashPassword(password);

            var user = new User
            {
                Username = username,
                Email = email,
                Password = hashedPassword,
                Phone = phone,
                Address = "",
                Role = "Admin"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "สร้างบัญชี Admin สำเร็จ";
            return RedirectToAction(nameof(Users));
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
