using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models.Entities;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace BookStore.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========== DASHBOARD ==========
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var dashboardData = new
            {
                TotalProducts = await _context.Products.CountAsync(),
                PendingProducts = await _context.Products.CountAsync(p => p.Status == "Pending"),
                ActiveProducts = await _context.Products.CountAsync(p => p.Status == "Active"),
                LowStockProducts = await _context.Products.CountAsync(p => p.StockQuantity < 10),
                RejectedProducts = await _context.Products.CountAsync(p => p.Status == "Rejected")
            };

            return View(dashboardData);
        }

        // ========== PENDING PRODUCTS ==========
        [HttpGet]
        public async Task<IActionResult> PendingProducts()
        {
            var pendingProducts = await _context.Products
                .Where(p => p.Status == "Pending")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(pendingProducts);
        }

        // ========== ALL PRODUCTS ==========
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var allProducts = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(allProducts);
        }

        // ========== ADD PRODUCT ==========
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(string Title, string Author, string Description, decimal Price, int StockQuantity = 10)
        {
            try
            {
                // الحصول على ID المستخدم
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found. Please login again.";
                    return RedirectToAction("AddProduct");
                }

                // تحديد الحالة حسب الدور
                string status = "Pending";
                if (User.IsInRole("SuperAdmin"))
                {
                    status = "Active"; // SuperAdmin يضيف مباشرة
                }

                // إنشاء المنتج يدوياً
                var product = new Product
                {
                    Title = Title,
                    Author = Author,
                    Description = Description,
                    Price = Price,
                    StockQuantity = StockQuantity,
                    Status = status,
                    AddedByUserId = userId,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                // حفظ في الداتابيز
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ Product '{Title}' added successfully! " +
                    (status == "Pending" ? "Waiting for SuperAdmin approval." : "Approved automatically.");

                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("AddProduct");
            }
        }

        // ========== APPROVE PRODUCT ==========
        [Authorize(Roles = "SuperAdmin")] // فقط SuperAdmin
        [HttpPost]
        public async Task<IActionResult> ApproveProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found!";
                return RedirectToAction("Products");
            }

            // تحقق أن المنتج في حالة Pending
            if (product.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending products can be approved.";
                return RedirectToAction("Products");
            }

            product.Status = "Active";
            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Product '{product.Title}' approved successfully!";
            return RedirectToAction("Products");
        }

        // ========== REJECT PRODUCT ==========
        [Authorize(Roles = "SuperAdmin")] // فقط SuperAdmin
        [HttpPost]
        public async Task<IActionResult> RejectProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found!";
                return RedirectToAction("Products");
            }

            // تحقق أن المنتج في حالة Pending
            if (product.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending products can be rejected.";
                return RedirectToAction("Products");
            }

            product.Status = "Rejected";
            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Product '{product.Title}' has been rejected.";
            return RedirectToAction("Products");
        }

        // ========== DELETE PRODUCT ==========
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found!";
                return RedirectToAction("Products");
            }

            // تحديد من له صلاحية الحذف
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // SuperAdmin يستطيع حذف أي منتج
            // Admin يستطيع حذف فقط منتجاته الخاصة
            if (User.IsInRole("SuperAdmin") ||
                (User.IsInRole("Admin") && product.AddedByUserId == userId))
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ Product '{product.Title}' deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this product.";
            }

            return RedirectToAction("Products");
        }

        // ========== EDIT PRODUCT ==========
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found!";
                return RedirectToAction("Products");
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, Product model)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found!";
                return RedirectToAction("Products");
            }

            // تحقق من الصلاحيات
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("SuperAdmin") && product.AddedByUserId != userId)
            {
                TempData["ErrorMessage"] = "You can only edit your own products.";
                return RedirectToAction("Products");
            }

            // تحديث البيانات
            product.Title = model.Title;
            product.Author = model.Author;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.UpdatedAt = DateTime.Now;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Product '{product.Title}' updated successfully!";
            return RedirectToAction("Products");
        }
    }
}