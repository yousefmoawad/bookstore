
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var cartViewModel = await GetCartViewModelAsync();
            return View(cartViewModel);
        }

        // GET: /Cart/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cartViewModel = await GetCartViewModelAsync();
            if (!cartViewModel.Items.Any())
            {
                TempData["InfoMessage"] = "Your cart is empty! Add some books first.";
                return RedirectToAction("Index");
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                Cart = cartViewModel,
                Email = User.Identity?.Name,
                PaymentMethod = "Credit Card"
            };

            return View(checkoutViewModel);
        }

        // POST: /Cart/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            try
            {
                var cartViewModel = await GetCartViewModelAsync();

                // Check if cart is empty
                if (!cartViewModel.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty!";
                    return RedirectToAction("Index");
                }

                model.Cart = cartViewModel;

                // Basic validation
                if (string.IsNullOrEmpty(model.ShippingAddress))
                {
                    ModelState.AddModelError("ShippingAddress", "Shipping address is required.");
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.PaymentMethod))
                {
                    ModelState.AddModelError("PaymentMethod", "Payment method is required.");
                    return View(model);
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FindAsync(userId);

                // Fill user info
                if (string.IsNullOrEmpty(model.Email) && user != null)
                    model.Email = user.Email;
                if (string.IsNullOrEmpty(model.PhoneNumber) && user != null)
                    model.PhoneNumber = user.PhoneNumber;

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = cartViewModel.Total,
                    ShippingAddress = model.ShippingAddress,
                    PaymentMethod = model.PaymentMethod,
                    OrderStatus = "Processing",
                    ShippingFee = cartViewModel.ShippingFee,
                    CreatedAt = DateTime.Now
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order items and update stock
                foreach (var item in cartViewModel.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PriceAtTime = item.Price,
                        CreatedAt = DateTime.Now
                    };
                    _context.OrderItems.Add(orderItem);

                    // Update product stock
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= item.Quantity;
                        if (product.StockQuantity < 0) product.StockQuantity = 0;
                        product.UpdatedAt = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                // ========== الأهم: إمساح الـ Cart وإظهار رسالة النجاح ==========
                HttpContext.Session.Remove("ShoppingCart");

                TempData["SuccessMessage"] = $"✅ Payment Successful! Order #{order.Id} has been placed. Thank you for your purchase!";

                // ========== إعادة التوجيه للـ Cart (هيكون فاضي) ==========
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Payment failed: {ex.Message}";
                return RedirectToAction("Checkout");
            }
        }
       


        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Status != "Active")
            {
                return Json(new { success = false, message = "Product not available" });
            }

            // Check stock
            if (product.StockQuantity < quantity)
            {
                return Json(new
                {
                    success = false,
                    message = $"Only {product.StockQuantity} items available in stock"
                });
            }

            var cart = GetCartItemsFromSession();

            // Check if product already in cart
            var cartItem = cart.FirstOrDefault(item => item.ProductId == productId);
            if (cartItem != null)
            {
                var newQuantity = cartItem.Quantity + quantity;
                if (newQuantity > product.StockQuantity)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Cannot add more than {product.StockQuantity} items total"
                    });
                }
                cartItem.Quantity = newQuantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = productId,
                    Title = product.Title,
                    Author = product.Author,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl,
                    Icon = product.Icon,
                    StockQuantity = product.StockQuantity
                });
            }

            SaveCartItemsToSession(cart);

            return Json(new
            {
                success = true,
                message = "Product added to cart",
                cartCount = cart.Sum(item => item.Quantity)
            });
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveItem(productId);
            }

            var product = await _context.Products.FindAsync(productId);
            if (product != null && quantity > product.StockQuantity)
            {
                TempData["ErrorMessage"] = $"Only {product.StockQuantity} items available in stock";
                return RedirectToAction("Index");
            }

            var cart = GetCartItemsFromSession();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                SaveCartItemsToSession(cart);
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var cart = GetCartItemsFromSession();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == productId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCartItemsToSession(cart);
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove("ShoppingCart");
            return RedirectToAction("Index");
        }

        // GET: /Cart/GetCartCount
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = GetCartItemsFromSession();
            var count = cart.Sum(item => item.Quantity);
            return Json(new { count });
        }

        // Helper Methods
        private List<CartItemViewModel> GetCartItemsFromSession()
        {
            var cartJson = HttpContext.Session.GetString("ShoppingCart");
            if (!string.IsNullOrEmpty(cartJson))
            {
                return JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson);
            }
            return new List<CartItemViewModel>();
        }

        private void SaveCartItemsToSession(List<CartItemViewModel> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("ShoppingCart", cartJson);
        }

        private async Task<CartViewModel> GetCartViewModelAsync()
        {
            var cartItems = GetCartItemsFromSession();

            // Update product info from database
            foreach (var item in cartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    item.Title = product.Title;
                    item.Author = product.Author;
                    item.Price = product.Price;
                    item.ImageUrl = product.ImageUrl;
                    item.Icon = product.Icon;
                    item.StockQuantity = product.StockQuantity;
                }
            }

            return new CartViewModel
            {
                Items = cartItems,
                ShippingFee = 5.99m
            };
        }
    }
}
