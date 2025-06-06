using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_BLOG.Data;
using QL_BLOG.Models; 

namespace QL_BLOG.Controllers
{
    public class AdminController : Controller
    {
        private readonly BlogDbContext _context;

        public AdminController(BlogDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ManagePosts()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            if (!isAdmin)
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            var posts = _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Create_At)
                .ToList();

            return View(posts);
        }

        [HttpGet]
        public IActionResult ManageUsers()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            if (!isAdmin)
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            var users = _context.Accounts
                .OrderByDescending(u => u.Create_At)
                .ToList();

            return View(users);
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Accounts.Find(id);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại.";
                return RedirectToAction("ManageUsers");
            }

            if (user.IsAdmin)
            {
                TempData["Error"] = "Không thể xóa tài khoản admin.";
                return RedirectToAction("ManageUsers");
            }

            try
            {
                // Xóa các bài viết của user trước
                var userPosts = _context.Posts.Where(p => p.Id_User == id);
                _context.Posts.RemoveRange(userPosts);
                
                // Xóa user
                _context.Accounts.Remove(user);
                _context.SaveChanges();

                TempData["Message"] = "Xóa người dùng thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa người dùng: " + ex.Message;
            }

            return RedirectToAction("ManageUsers");
        }
    }
}