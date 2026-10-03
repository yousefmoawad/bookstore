using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    public class Order : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ShippingAddress { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Credit Card";

        [Required]
        [MaxLength(20)]
        public string OrderStatus { get; set; } = "Pending";

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; } = 5.99m;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}