using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_BLOG.Data;
using QL_BLOG.Models;
using QL_BLOG.Models.ViewModels;

namespace QL_BLOG.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlogDbContext _context;

        public HomeController(BlogDbContext context)
        {
            _context = context;
        }

        // Trang chủ
        public IActionResult Index()
        {
            var topUsers = _context.Posts
                .GroupBy(p => p.Id_User)
                .Select(g => new 
                {
                    UserId = g.Key,
                    PostCount = g.Count()
                })
                .OrderByDescending(x => x.PostCount)
                .Take(5)
                .Join(_context.Accounts,
                      g => g.UserId,
                      u => u.Id_User,
                      (g, u) => new TopUserViewModel
                      {
                          Username = u.Username,
                          PostCount = g.PostCount,
                          Create_At = u.Create_At
                      })
                .ToList();

            ViewBag.TopUsers = topUsers;

            // Lấy 1 bài viết ngẫu nhiên cùng Username
            var randomPost = _context.Posts
                .OrderBy(r => Guid.NewGuid())
                .Join(_context.Accounts,
                      p => p.Id_User,
                      a => a.Id_User,
                      (p, a) => new PostViewModel
                      {
                          Id_Post = p.Id_Post,
                          Topic = p.Topic,
                          Content = p.Content,
                          Create_At = p.Create_At,
                          Username = a.Username,
                          Image_Posted = p.Image_Posted // Thêm dòng này
                      })
                .FirstOrDefault();

            ViewBag.RandomPosts = randomPost;

            // Lấy danh sách bài viết, bao gồm thông tin người dùng
            var posts = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Account) // nếu cần hiển thị tên người đăng
                .OrderByDescending(p => p.Create_At)
                .ToList();

            return View(posts);
        }

        // Chi tiết bài viết
        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Where(p => p.Id_Post == id)
                .Select(p => new
                {
                    p.Id_Post,
                    p.Topic,
                    p.Content,
                    p.Image_Posted,
                    p.Create_At,
                    Account = _context.Accounts.FirstOrDefault(a => a.Id_User == p.Id_User)
                })
                .FirstOrDefault();

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // Tìm kiếm bài viết
        public IActionResult Search(string searchTerm)
        {
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.SearchPerformed = true;

            // Lấy danh sách category cho gợi ý
            ViewBag.Categories = _context.Categories
                .Select(c => c.Name_Category)
                .ToList();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction("Index");
            }

            var posts = _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Category)
                .Where(p => p.Topic.Contains(searchTerm) ||
                           p.Content.Contains(searchTerm) ||
                           p.Account.Username.Contains(searchTerm) ||
                           p.Category.Name_Category.Contains(searchTerm))
                .OrderByDescending(p => p.Create_At)
                .ToList();

            ViewBag.NoResults = !posts.Any();

            // Lấy top users và random post như trang Index
            SetupSidebarData();

            return View("Index", posts);
        }

        private void SetupSidebarData()
        {
            var topUsers = _context.Posts
                .GroupBy(p => p.Id_User)
                .Select(g => new 
                {
                    UserId = g.Key,
                    PostCount = g.Count()
                })
                .OrderByDescending(x => x.PostCount)
                .Take(5)
                .Join(_context.Accounts,
                      g => g.UserId,
                      u => u.Id_User,
                      (g, u) => new TopUserViewModel
                      {
                          Username = u.Username,
                          PostCount = g.PostCount,
                          Create_At = u.Create_At
                      })
                .ToList();

            ViewBag.TopUsers = topUsers;

            // Lấy 1 bài viết ngẫu nhiên cùng Username
            var randomPost = _context.Posts
                .OrderBy(r => Guid.NewGuid())
                .Join(_context.Accounts,
                      p => p.Id_User,
                      a => a.Id_User,
                      (p, a) => new PostViewModel
                      {
                          Id_Post = p.Id_Post,
                          Topic = p.Topic,
                          Content = p.Content,
                          Create_At = p.Create_At,
                          Username = a.Username,
                          Image_Posted = p.Image_Posted // Thêm dòng này
                      })
                .FirstOrDefault();

            ViewBag.RandomPosts = randomPost;
        }
    }
}