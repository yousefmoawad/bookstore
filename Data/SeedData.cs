using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BookStore.Models.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // تأكد من إنشاء الداتابيز
            await context.Database.MigrateAsync();

            // إنشاء الأدوار
            string[] roles = { "SuperAdmin", "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // إنشاء SuperAdmin
            var adminEmail = "superadmin@bookstore.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "SuperAdmin@2024");

                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(adminUser, new[] { "SuperAdmin", "Admin" });
                }
            }

            // إضافة منتجات تجريبية إذا مفيش منتجات
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product
                    {
                        Title = "The Great Gatsby",
                        Author = "F. Scott Fitzgerald",
                        Description = "A classic novel about the American Dream",
                        Price = 12.99m,
                        StockQuantity = 100,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        AddedByUserId = adminUser.Id
                    },
                    new Product
                    {
                        Title = "To Kill a Mockingbird",
                        Author = "Harper Lee",
                        Description = "A novel about racial injustice",
                        Price = 14.99m,
                        StockQuantity = 80,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        AddedByUserId = adminUser.Id
                    },
                    new Product
                    {
                        Title = "1984",
                        Author = "George Orwell",
                        Description = "A dystopian social science fiction novel",
                        Price = 10.99m,
                        StockQuantity = 120,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        AddedByUserId = adminUser.Id
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}