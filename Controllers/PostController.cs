using Microsoft.AspNetCore.Mvc;
using QL_BLOG.Data;
using QL_BLOG.Models;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace QL_BLOG.Controllers
{
    public class PostController : Controller
    {
        private readonly BlogDbContext _context;
        private readonly IImageHelper _imageHelper;

        public PostController(BlogDbContext context, IImageHelper imageHelper)
        {
            _context = context;
            _imageHelper = imageHelper;
        }

        // POST: /Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PostViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đăng bài viết.";
                return RedirectToAction("Login", "Account");
            }

            string imageFileName = _imageHelper.SaveImage(model.Image, userId.Value);

            var post = new Post
            {
                Id_User = userId.Value,
                Id_Category = model.Id_Category,
                Topic = model.Topic,
                Content = model.Content,
                Create_At = DateTime.Now,
                Image_Posted = imageFileName ?? "default.jpg",
                States = 1,
                View_Number = 0
            };

            _context.Posts.Add(post);
            _context.SaveChanges();

            TempData["Message"] = "Bài viết đã được đăng thành công!";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Post/Edit/{id}
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            var categories = _context.Categories.ToList();
            ViewBag.Categories = categories;

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var post = _context.Posts.FirstOrDefault(p => p.Id_Post == id);
            if (post == null)
            {
                TempData["Error"] = "Bài viết không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra quyền: chỉ admin hoặc chủ sở hữu bài viết mới được chỉnh sửa
            if (!isAdmin && post.Id_User != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa bài viết này!";
                return RedirectToAction("Index", "Home");
            }

            // Chuyển đổi từ Post sang PostViewModel
            var postViewModel = new PostViewModel
            {
                Id_Post = post.Id_Post,
                Topic = post.Topic,
                Content = post.Content,
                Image_Posted = post.Image_Posted,
                Id_User = post.Id_User,
                Id_Category = post.Id_Category
            };

            return View(postViewModel);
        }

        // POST: /Post/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PostViewModel model)
        {
            if (ModelState.IsValid)
            {
                var post = _context.Posts.FirstOrDefault(p => p.Id_Post == model.Id_Post);
                if (post == null)
                {
                    TempData["Error"] = "Bài viết không tồn tại.";
                    return RedirectToAction("Index", "Home");
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                
                // Cập nhật dữ liệu từ ViewModel
                post.Topic = model.Topic;
                post.Content = model.Content;
                post.Id_Category = model.Id_Category;

                // Nếu có ảnh mới thì cập nhật, không thì giữ nguyên ảnh cũ
                if (model.Image != null)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(post.Image_Posted) && post.Image_Posted != "default.jpg")
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", post.Image_Posted);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Lưu ảnh mới
                    var userFolder = EnsureUserImageDirectory(userId.Value);
                    var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                    var filePath = Path.Combine(userFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.Image.CopyTo(stream);
                    }

                    post.Image_Posted = $"user_{userId}/{fileName}";
                }
                // Nếu không có ảnh mới, giữ nguyên post.Image_Posted

                _context.SaveChanges();

                TempData["Message"] = "Chỉnh sửa bài viết thành công.";
                return RedirectToAction("Index", "Home");
            }

            // Đảm bảo luôn trả về một IActionResult
            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }

        // POST: /Post/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var post = _context.Posts.FirstOrDefault(p => p.Id_Post == id);
            if (post == null)
            {
                TempData["Error"] = "Bài viết không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra quyền xóa: chỉ admin hoặc chủ bài viết mới được xóa
            if (!isAdmin && post.Id_User != userId)
            {
                TempData["Error"] = "Bạn không có quyền xóa bài viết này!";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Xóa file ảnh nếu có
                if (!string.IsNullOrEmpty(post.Image_Posted) && post.Image_Posted != "default.jpg")
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", post.Image_Posted);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Posts.Remove(post);
                _context.SaveChanges();

                TempData["Message"] = "Xóa bài viết thành công!";
                return RedirectToAction("Index", "Home");  // Chuyển hướng về trang chủ
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa bài viết: " + ex.Message;
                return RedirectToAction("Index", "Home");  // Chuyển hướng về trang chủ ngay cả khi có lỗi
            }
        }

        // GET: /Post/Details/{id}
        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id_Post == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        private string EnsureUserImageDirectory(int userId)
        {
            var imagesRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            var userFolder = Path.Combine(imagesRoot, $"user_{userId}");
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }
            return userFolder;
        }
    }
}
