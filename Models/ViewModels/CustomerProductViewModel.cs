using System.ComponentModel.DataAnnotations;

namespace BookStore.Models.ViewModels
{
    public class CustomerProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Writer name is required")]
        [MaxLength(200, ErrorMessage = "Writer name cannot exceed 200 characters")]
        [Display(Name = "Writer/Author")]
        public string Writer { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
        public decimal Price { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; }

        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Or Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? ImageUrlInput { get; set; }

        public string? ImageUrl { get; set; }
        public string ApprovalStatus { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }
}