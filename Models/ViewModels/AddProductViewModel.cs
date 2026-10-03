
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models.ViewModels
{
    public class AddProductViewModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        [MaxLength(200)]
        public string Author { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000.")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 10;
    }
}
