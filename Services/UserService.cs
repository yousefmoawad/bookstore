using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ========== GET METHODS ==========
        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // ========== COUNT METHODS ==========
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.IsActive);
        }

        // ========== ROLE METHODS ==========
        public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role)
        {
            // Get user IDs in this role
            var userIdsInRole = await _userManager.GetUsersInRoleAsync(role);
            var userIds = userIdsInRole.Select(u => u.Id).ToList();

            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove all current roles
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                // Add new role
                var result = await _userManager.AddToRoleAsync(user, role);

                // Update user's Role property
                if (result.Succeeded)
                {
                    user.Role = role;
                    await _userManager.UpdateAsync(user);
                }

                return result.Succeeded;
            }
            catch
            {
                return false;
            }
        }

        // ========== CRUD METHODS ==========
        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                if (user == null)
                    return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                    return false;

                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                return await UpdateUserAsync(user);
            }
            catch
            {
                return false;
            }
        }

        // ========== SEARCH METHODS ==========
        public async Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllUsersAsync();

            return await _context.Users
                .Where(u => u.FirstName.Contains(searchTerm) ||
                           u.LastName.Contains(searchTerm) ||
                           u.Email.Contains(searchTerm) ||
                           u.UserName.Contains(searchTerm) ||
                           u.PhoneNumber.Contains(searchTerm))
                .ToListAsync();
        }

        // ========== CART METHODS ==========
        public async Task<string> GetUserCartAsync(string userId)
        {
            // For now, return empty string or implement cart logic
            return string.Empty;
        }

        public async Task<bool> SaveUserCartAsync(string userId, string cartData)
        {
            try
            {
                // Implement cart saving logic here
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ClearUserCartAsync(string userId)
        {
            try
            {
                // Implement cart clearing logic here
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ========== ADDITIONAL HELPER METHODS ==========
        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            return (await _userManager.GetRolesAsync(user)).ToList();
        }

        public async Task<bool> IsInRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<bool> CreateUserAsync(ApplicationUser user, string password, string role = "Customer")
        {
            try
            {
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                    user.Role = role;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}