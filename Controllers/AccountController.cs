using Microsoft.AspNetCore.Mvc;
using QL_BLOG.Data;
using QL_BLOG.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace QL_BLOG.Controllers
{
    public class AccountController : Controller
    {
        private readonly BlogDbContext _context;

        public AccountController(BlogDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var hashedPassword = HashPassword(password);
            var user = _context.Accounts.FirstOrDefault(a => a.Username == username && a.Password == hashedPassword);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id_User);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin ? "true" : "false");
                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        // Hàm mã hóa mật khẩu (cùng hàm với trong Program.cs)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa trạng thái đăng nhập
            return RedirectToAction("Login");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            // Trả về view với model đúng kiểu
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Accounts.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                var user = new Account
                {
                    Username = model.Username,
                    Password = HashPassword(model.Password),
                    Email = model.Email,
                    Gender = model.Gender,
                    IsAdmin = false,
                    Create_At = DateTime.Now
                };

                _context.Accounts.Add(user);
                _context.SaveChanges();

                TempData["Message"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Trang cá nhân
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Accounts.FirstOrDefault(u => u.Id_User == userId);
            var userPosts = _context.Posts
                .Include(p => p.Category)
                .Where(p => p.Id_User == userId)
                .ToList();

            ViewBag.UserPosts = userPosts;
            ViewBag.UserId = userId;
            ViewBag.UserInfo = user;

            return View();
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Accounts.FirstOrDefault(u => u.Id_User == userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(Account model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Accounts.FirstOrDefault(u => u.Id_User == model.Id_User);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.Gender = model.Gender;
                user.Birthday = model.Birthday;

                _context.SaveChanges();
                TempData["Message"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }
            return View(model);
        }
    }
}