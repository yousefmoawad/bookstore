using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    public class CustomerProduct : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string Writer { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public string? ApprovedByAdminId { get; set; }

        [MaxLength(20)]
        public string ApprovalStatus { get; set; } = "Pending";

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("ApprovedByAdminId")]
        public virtual ApplicationUser? ApprovedByAdmin { get; set; }
    }
}