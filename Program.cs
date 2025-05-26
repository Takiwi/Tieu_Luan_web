using Microsoft.EntityFrameworkCore;
using QL_BLOG.Data;
using System.Security.Cryptography;
using System.Text;

namespace QL_BLOG
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Đăng ký DbContext
            builder.Services.AddDbContext<BlogDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Cấu hình dịch vụ Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Đăng ký HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Cấu hình Authentication
            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", options =>
                {
                    options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập
                    options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn khi bị từ chối truy cập
                });

            builder.Services.AddScoped<IImageHelper, ImageHelper>();

            var app = builder.Build();

            // Kích hoạt Session
            app.UseSession();
            app.UseStaticFiles();

            // Tạo tài khoản admin mặc định
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

                // Kiểm tra nếu chưa có tài khoản admin
                if (!context.Accounts.Any(a => a.IsAdmin))
                {
                    var adminAccount = new Account
                    {
                        Username = "admin",
                        Password = HashPassword("admin123"), // Mã hóa mật khẩu
                        Email = "admin@example.com",
                        Gender = "M", // Cung cấp giá trị cho Gender (ví dụ: "M" cho Nam, "F" cho Nữ)
                        IsAdmin = true,
                        Create_At = DateTime.Now
                    };

                    context.Accounts.Add(adminAccount);
                    context.SaveChanges();
                }
            }

            // Ánh xạ các controller
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }

        // Hàm mã hóa mật khẩu
        static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
