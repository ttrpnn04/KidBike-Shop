using Microsoft.AspNetCore.Mvc;
using CSI402_Project.Models.Db;
using CSI402_Project.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace CSI402_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly BikeShopDbContext _context;

        public AccountController(BikeShopDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = _context.Users.FirstOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "ชื่อผู้ใช้นี้ถูกใช้งานแล้ว");
                    return View(model);
                }

                // Check if email already exists
                var existingEmail = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "อีเมลนี้ถูกใช้งานแล้ว");
                    return View(model);
                }

                // Create new user
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = HashPassword(model.Password),
                    Phone = model.Phone,
                    Address = model.Address,
                    Role = "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Set success message
                TempData["SuccessMessage"] = "สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ";

                // Redirect to login page (we'll create this later or redirect to home)
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = HashPassword(model.Password);
                var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == hashedPassword);

                if (user == null)
                {
                    ModelState.AddModelError("", "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง");
                    return View(model);
                }

                // Store user info in session
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role ?? "User");

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Helper method to hash password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
