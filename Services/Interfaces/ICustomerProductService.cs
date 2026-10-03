using BookStore.Models.Entities;
using BookStore.Models.ViewModels;

namespace BookStore.Services.Interfaces
{
    public interface ICustomerProductService
    {
        Task<CustomerProduct> CreateCustomerProductAsync(CustomerProductViewModel model, string userId);
        Task<IEnumerable<CustomerProduct>> GetCustomerProductsAsync(string userId);
        Task<IEnumerable<CustomerProduct>> GetAllPendingProductsAsync();
        Task<bool> ApproveProductAsync(int productId, string adminId);
        Task<bool> RejectProductAsync(int productId, string adminId);
        Task<bool> DeleteCustomerProductAsync(int id);
        Task<int> GetPendingProductsCountAsync();
    }
}