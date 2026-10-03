using BookStore.Models.Entities;

namespace BookStore.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(string name, string description);
        Task<bool> UpdateCategoryAsync(int id, string name, string description);
        Task<bool> DeleteCategoryAsync(int id);
    }
}