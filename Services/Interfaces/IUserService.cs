using BookStore.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookStore.Services.Interfaces
{
    public interface IUserService
    {
        // ========== GET METHODS ==========
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser> GetUserByIdAsync(string id);
        Task<ApplicationUser> GetUserByEmailAsync(string email);

        // ========== COUNT METHODS ==========
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();

        // ========== ROLE METHODS ==========
        Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role);
        Task<bool> AssignRoleAsync(string userId, string role);

        // ========== CRUD METHODS ==========
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> ToggleUserStatusAsync(string userId);

        // ========== SEARCH METHODS ==========
        Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm);

        // ========== CART METHODS ==========
        Task<string> GetUserCartAsync(string userId);
        Task<bool> SaveUserCartAsync(string userId, string cartData);
        Task<bool> ClearUserCartAsync(string userId);

        // ========== OPTIONAL HELPER METHODS ==========
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<bool> CreateUserAsync(ApplicationUser user, string password, string role = "Customer");
    }
}