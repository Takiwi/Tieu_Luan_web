using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_BLOG.Data
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Thiết lập mối quan hệ giữa Post và Account
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Account)
                .WithMany()
                .HasForeignKey(p => p.Id_User)
                .OnDelete(DeleteBehavior.Restrict);

            // Thiết lập mối quan hệ giữa Post và Category
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.Id_Category)
                .OnDelete(DeleteBehavior.Restrict);  // Thêm ràng buộc xóa

            // Thiết lập mối quan hệ giữa Comment và Post
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany()
                .HasForeignKey(c => c.Id_Post)
                .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập mối quan hệ giữa Comment và Account
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Account)
                .WithMany()
                .HasForeignKey(c => c.Id_User)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }

    public class Account
    {
        [Key]
        public int Id_User { get; set; }
        [MaxLength(50)]
        public string Username { get; set; }
        public DateTime? Birthday { get; set; }
        [MaxLength(3)]
        public string Gender { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(50)]
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime Create_At { get; set; }
    }

    public class Category
    {
        [Key]
        public int Id_Category { get; set; }
        
        [MaxLength(50)]
        public string Name_Category { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }

    public class Post
    {
        [Key]
        public int Id_Post { get; set; }
        public int Id_User { get; set; }
        public int Id_Category { get; set; }

        [Required(ErrorMessage = "Chủ đề là bắt buộc.")]
        public string Topic { get; set; }

        [Required(ErrorMessage = "Nội dung là bắt buộc.")]
        public string Content { get; set; }

        public string? Image_Posted { get; set; }

        public DateTime Create_At { get; set; }

        public byte States { get; set; }

        [DefaultValue(0)]  
        public int View_Number { get; set; }

        public Account Account { get; set; }

        [ForeignKey("Id_Category")]
        public virtual Category Category { get; set; }
    }

    public class Comment
    {
        [Key]
        public int Id_Comment { get; set; }

        // Khóa ngoại liên kết với Post
        public int Id_Post { get; set; }
        public Post Post { get; set; }

        // Khóa ngoại liên kết với Account
        public int Id_User { get; set; }
        public Account Account { get; set; }

        public DateTime Create_At { get; set; }
    }
}