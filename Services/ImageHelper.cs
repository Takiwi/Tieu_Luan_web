using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

public interface IImageHelper
{
    string SaveImage(IFormFile image, int userId);
    void DeleteImage(string imagePath);
    string EnsureUserImageDirectory(int userId);
}

public class ImageHelper : IImageHelper
{
    private readonly string _webRootPath;

    public ImageHelper(IWebHostEnvironment webHostEnvironment)
    {
        _webRootPath = webHostEnvironment.WebRootPath;
    }

    public string SaveImage(IFormFile image, int userId)
    {
        if (image == null) return null;

        var userFolder = EnsureUserImageDirectory(userId);
        var fileName = $"{Guid.NewGuid()}_{image.FileName}";
        var filePath = Path.Combine(userFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            image.CopyTo(stream);
        }

        return $"user_{userId}/{fileName}";
    }

    public void DeleteImage(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || imagePath == "default.jpg") return;

        var fullPath = Path.Combine(_webRootPath, "images", imagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public string EnsureUserImageDirectory(int userId)
    {
        var userFolder = Path.Combine(_webRootPath, "images", $"user_{userId}");
        if (!Directory.Exists(userFolder))
        {
            Directory.CreateDirectory(userFolder);
        }
        return userFolder;
    }
}