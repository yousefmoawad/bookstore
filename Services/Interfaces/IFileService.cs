using Microsoft.AspNetCore.Http;

namespace BookStore.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile imageFile, string subFolder);
        Task<string> SaveImageFromUrlAsync(string imageUrl, string subFolder);
        void DeleteImage(string imagePath);
        bool IsImageValid(IFormFile imageFile);
    }
}