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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "ชื่อผู้ใช้นี้ถูกใช้งานแล้ว");
                    return View(model);
                }

                var existingEmail = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "อีเมลนี้ถูกใช้งานแล้ว");
                    return View(model);
                }

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
                _context.SaveChanges();

                TempData["SuccessMessage"] = "สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ";

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
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

                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role ?? "User");

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (user == null)
                {
                    TempData["SuccessMessage"] = "หากอีเมลนี้มีในระบบ เราจะส่งลิงก์รีเซ็ตรหัสผ่านไปให้";
                    return RedirectToAction("Login");
                }

                TempData["SuccessMessage"] = "หากอีเมลนี้มีในระบบ เราจะส่งลิงก์รีเซ็ตรหัสผ่านไปให้ กรุณาตรวจสอบอีเมลของคุณ (รวมถึงกล่องจดหมายขยะ)";
                return RedirectToAction("Login");
            }

            return View(model);
        }

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
