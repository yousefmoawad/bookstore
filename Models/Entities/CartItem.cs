using BookStore.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CartItem : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ShoppingCartId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; } = 1;

    // Navigation properties
    [ForeignKey("ShoppingCartId")]
    public virtual ShoppingCart ShoppingCart { get; set; }

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; }
}
