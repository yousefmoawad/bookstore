using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.ViewModels;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == "Active")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == "Active");
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 4)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == "Active" && p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.Status == "Active")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Product> CreateProductAsync(ProductViewModel model, string userId, IFormFile? imageFile = null)
        {
            var product = new Product
            {
                Title = model.Title,
                Author = model.Author,
                Description = model.Description,
                Price = model.Price,
                ImageUrl = model.ImageUrl,
                Icon = model.Icon,
                StockQuantity = model.StockQuantity,
                CategoryId = model.CategoryId,
                AddedByUserId = userId,
                IsFeatured = model.IsFeatured,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Product> UpdateProductAsync(int id, ProductViewModel model, IFormFile? imageFile = null)
        {
            var product = await GetProductByIdAsync(id);
            if (product == null) return null;

            product.Title = model.Title;
            product.Author = model.Author;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Icon = model.Icon;
            product.StockQuantity = model.StockQuantity;
            product.CategoryId = model.CategoryId;
            product.IsFeatured = model.IsFeatured;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await GetProductByIdAsync(id);
            if (product == null) return false;

            product.Status = "Inactive";
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllProductsAsync();

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == "Active" &&
                           (p.Title.Contains(searchTerm) ||
                            p.Author.Contains(searchTerm) ||
                            p.Description.Contains(searchTerm)))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetNewArrivalsAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == "Active")
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantityChange)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.StockQuantity += quantityChange;
            if (product.StockQuantity < 0) product.StockQuantity = 0;

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}