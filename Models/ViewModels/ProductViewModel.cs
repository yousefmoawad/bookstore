using System.ComponentModel.DataAnnotations;

namespace BookStore.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [MaxLength(200, ErrorMessage = "Author name cannot exceed 200 characters")]
        public string Author { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
        public string Icon { get; set; } = "📚";

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; } = 10;

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        public bool IsFeatured { get; set; }

        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Or Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? ImageUrlInput { get; set; }
    }
}