
﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BookStore.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        [PersonalData]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [PersonalData]
        public string LastName { get; set; }

        [MaxLength(255)]
        [PersonalData]
        public string? AvatarUrl { get; set; }

        [MaxLength(500)]
        [PersonalData]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string Role { get; set; } = "Customer";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Product> AddedProducts { get; set; } = new List<Product>();

        // علّق أو احذف هذه الخاصية إذا مش موجودة في Product
        // public virtual ICollection<Product> ApprovedProducts { get; set; } = new List<Product>();
    }
}
