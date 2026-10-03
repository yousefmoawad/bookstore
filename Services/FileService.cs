using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BookStore.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, string subFolder)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                    return null;

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return $"/uploads/{subFolder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image");
                return null;
            }
        }

        public async Task<string> SaveImageFromUrlAsync(string imageUrl, string subFolder)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return null;

                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}.jpg";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await File.WriteAllBytesAsync(filePath, imageBytes);

                return $"/uploads/{subFolder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image from URL");
                return null;
            }
        }

        public void DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath) || !imagePath.StartsWith("/uploads/"))
                    return;

                var filePath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
            }
        }

        public bool IsImageValid(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                return false;

            if (imageFile.Length > 5 * 1024 * 1024) // 5MB limit
                return false;

            return true;
        }
    }
}