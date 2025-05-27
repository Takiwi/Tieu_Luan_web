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
            // Lấy dữ liệu cho sidebar trước
            SetupSidebarData();

            var posts = _context.Posts
                .Include(p => p.Category)  
                .Include(p => p.Account)
                .OrderByDescending(p => p.Create_At)
                .ToList();

            return View(posts);
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

        // Chi tiết bài viết
        public IActionResult Details(int id)
        {
            try
            {
                // First get the post
                var post = _context.Posts
                    .Include(p => p.Account)
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.Id_Post == id);

                if (post == null)
                {
                    return NotFound();
                }

                // Update view count using direct SQL update
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Use parameterized SQL to prevent SQL injection
                        var sql = "UPDATE Posts SET View_Number = COALESCE(View_Number, 0) + 1 WHERE Id_Post = @p0";
                        _context.Database.ExecuteSqlRaw(sql, id);
                        
                        // Commit the transaction
                        transaction.Commit();
                        
                        // Force refresh the post from database
                        _context.Entry(post).State = EntityState.Detached;
                        post = _context.Posts
                            .Include(p => p.Account)
                            .Include(p => p.Category)
                            .FirstOrDefault(p => p.Id_Post == id);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                SetupSidebarData();
                return View(post);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating view count: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // Cập nhật phần lấy random post để include Category
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
                          Id_User = u.Id_User,  
                          Username = u.Username,
                          PostCount = g.PostCount,
                          Create_At = u.Create_At
                      })
                .ToList();

            ViewBag.TopUsers = topUsers;

            // Thêm Include để lấy thông tin Category cho random post
            var randomPost = _context.Posts
                .Include(p => p.Category)
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
                          Image_Posted = p.Image_Posted,
                          CategoryName = p.Category.Name_Category
                      })
                .FirstOrDefault();

            ViewBag.RandomPosts = randomPost;
        }
    }
}