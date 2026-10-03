
﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BookStore.Models.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        [MaxLength(200)]
        public string Author { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000.")]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(10)]
        public string Icon { get; set; } = "📚";

        public int StockQuantity { get; set; } = 10;

        public int? CategoryId { get; set; }

        [Required]
        public string AddedByUserId { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Active, Rejected

        public bool IsFeatured { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("AddedByUserId")]
        public virtual ApplicationUser AddedByUser { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // احذف أو علّق هذه الخصائص إذا مش موجودة في الداتابيز
        // public DateTime? ApprovedAt { get; set; }
        // public string? ApprovedByUserId { get; set; }
        // [ForeignKey("ApprovedByUserId")]
        // public virtual ApplicationUser? ApprovedByUser { get; set; }
    }
}
