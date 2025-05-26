namespace QL_BLOG.Models.ViewModels
{
    // ViewModel cho người dùng có nhiều bài viết nhất
    public class TopUserViewModel
    {
        public string Username { get; set; }
        public int PostCount { get; set; }
        public DateTime Create_At { get; set; }
        public string? Image_Posted { get; set; } // Hình ảnh người dùng đã lưu
        public IFormFile Image { get; set; } // Tệp hình ảnh người dùng được tải lên
        public int Id_User { get; set; } // ID người dùng
    }
}