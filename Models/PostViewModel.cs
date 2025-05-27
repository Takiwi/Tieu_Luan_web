using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using QL_BLOG.Data; // Verify that the 'Data' folder exists and contains the necessary classes.

namespace QL_BLOG.Models
{
    public class PostViewModel
    {
        public int Id_Post { get; set; } // ID bài viết

        [Required(ErrorMessage = "Chủ đề là bắt buộc.")]
        public string Topic { get; set; } // Chủ đề bài viết

        [Required(ErrorMessage = "Nội dung là bắt buộc.")]
        public string Content { get; set; } // Nội dung bài viết

        public string? Image_Posted { get; set; } // Hình ảnh bài viết đã lưu

        public IFormFile? Image { get; set; } // Tệp hình ảnh được tải lên

        public int Id_User { get; set; } // ID người dùng

        public string? Username { get; set; } // Tên người dùng

        public DateTime Create_At { get; set; } // Ngày tạo bài viết

        [Required(ErrorMessage = "Danh mục là bắt buộc.")]
        public int Id_Category { get; set; }

        public byte States { get; set; }
        
        public int View_Number { get; set; } // Số lượt xem bài viết
        public string? CategoryName { get; set; } // Tên danh mục bài viết
    }
}