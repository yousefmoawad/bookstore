using System.ComponentModel.DataAnnotations;

namespace BookStore.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public string Icon { get; set; }
        public int StockQuantity { get; set; }
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal Subtotal => Items.Sum(i => i.Price * i.Quantity);
        public decimal ShippingFee { get; set; } = 5.99m;
        public decimal Total => Subtotal + ShippingFee;
        public int ItemCount => Items.Sum(i => i.Quantity);
    }

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Shipping address is required")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; } = "Credit Card";

        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public CartViewModel Cart { get; set; }
    }
}