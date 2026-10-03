using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.ViewModels;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Services
{
    public class CustomerProductService : ICustomerProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<CustomerProductService> _logger;

        public CustomerProductService(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<CustomerProductService> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<CustomerProduct> CreateCustomerProductAsync(CustomerProductViewModel model, string userId)
        {
            try
            {
                string imageUrl = null;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imageUrl = await _fileService.SaveImageAsync(model.ImageFile, "customer-products");
                }
                else if (!string.IsNullOrEmpty(model.ImageUrlInput))
                {
                    imageUrl = await _fileService.SaveImageFromUrlAsync(model.ImageUrlInput, "customer-products");
                }

                var customerProduct = new CustomerProduct
                {
                    UserId = userId,
                    Name = model.Name,
                    Writer = model.Writer,
                    Price = model.Price,
                    Description = model.Description,
                    ImageUrl = imageUrl,
                    ApprovalStatus = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _context.CustomerProducts.AddAsync(customerProduct);
                await _context.SaveChangesAsync();

                return customerProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer product");
                throw;
            }
        }

        public async Task<IEnumerable<CustomerProduct>> GetCustomerProductsAsync(string userId)
        {
            return await _context.CustomerProducts
                .Include(cp => cp.User)
                .Where(cp => cp.UserId == userId)
                .OrderByDescending(cp => cp.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerProduct>> GetAllPendingProductsAsync()
        {
            return await _context.CustomerProducts
                .Include(cp => cp.User)
                .Where(cp => cp.ApprovalStatus == "Pending")
                .OrderByDescending(cp => cp.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ApproveProductAsync(int productId, string adminId)
        {
            try
            {
                var product = await _context.CustomerProducts.FindAsync(productId);
                if (product == null) return false;

                product.ApprovalStatus = "Approved";
                product.ApprovedByAdminId = adminId;
                product.UpdatedAt = DateTime.UtcNow;

                // Add to main products
                var mainProduct = new Product
                {
                    Title = product.Name,
                    Author = product.Writer,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Icon = "📖",
                    StockQuantity = 10,
                    CategoryId = 1, // Default to Fiction category
                    AddedByUserId = product.UserId,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Products.AddAsync(mainProduct);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving product");
                return false;
            }
        }

        public async Task<bool> RejectProductAsync(int productId, string adminId)
        {
            try
            {
                var product = await _context.CustomerProducts.FindAsync(productId);
                if (product == null) return false;

                product.ApprovalStatus = "Rejected";
                product.ApprovedByAdminId = adminId;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting product");
                return false;
            }
        }

        public async Task<bool> DeleteCustomerProductAsync(int id)
        {
            try
            {
                var product = await _context.CustomerProducts.FindAsync(id);
                if (product == null) return false;

                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    _fileService.DeleteImage(product.ImageUrl);
                }

                _context.CustomerProducts.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer product");
                return false;
            }
        }

        public async Task<int> GetPendingProductsCountAsync()
        {
            return await _context.CustomerProducts
                .CountAsync(cp => cp.ApprovalStatus == "Pending");
        }
    }
}