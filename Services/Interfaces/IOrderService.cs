using BookStore.Models.Entities;
using BookStore.Models.ViewModels;

namespace BookStore.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CartViewModel cart, string userId, string shippingAddress, string paymentMethod);
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order> GetOrderByIdAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetTotalOrdersCountAsync();
    }
}