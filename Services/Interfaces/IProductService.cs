using BookStore.Models.Entities;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BookStore.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 4);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<Product> CreateProductAsync(ProductViewModel model, string userId, IFormFile? imageFile = null);
        Task<Product> UpdateProductAsync(int id, ProductViewModel model, IFormFile? imageFile = null);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<Product>> GetNewArrivalsAsync(int count = 8);
        Task<bool> UpdateStockAsync(int productId, int quantityChange);
    }
}