
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models.Entities;
using System.Threading.Tasks;
using System.Linq;

namespace BookStore.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Shop
        public async Task<IActionResult> Index()
        {
            // عرض فقط المنتجات الـ Active في Shop
            var products = await _context.Products
                .Where(p => p.Status == "Active") // فقط الـ Active
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        // GET: /Shop/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // تفاصيل المنتج (فقط إذا كان Active)
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == "Active");

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
