using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".BookStore.Session";
});

// Add MVC services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register application services - بس الأساسيات
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();

// تعليق الـ Services الزائدة مؤقتاً
// builder.Services.AddScoped<IOrderService, OrderService>();
// builder.Services.AddScoped<IFileService, FileService>();
// builder.Services.AddScoped<ICustomerProductService, CustomerProductService>();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Create upload directories
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
var avatarsPath = Path.Combine(uploadsPath, "avatars");
var productsPath = Path.Combine(uploadsPath, "products");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    Directory.CreateDirectory(avatarsPath);
    Directory.CreateDirectory(productsPath);
    Console.WriteLine("✅ مجلدات التحميل أنشئت بنجاح!");
}

// Create SuperAdmin and Admin and roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Create roles
        string[] roles = { "SuperAdmin", "Admin", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"✅ تم إنشاء الدور '{role}'.");
            }
        }

        // Create SuperAdmin
        var superAdminEmail = "superadmin@bookstore.com";
        var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

        if (superAdminUser == null)
        {
            superAdminUser = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true,
                Role = "SuperAdmin"
            };

            var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@2024");

            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(superAdminUser, new[] { "SuperAdmin", "Admin" });
                Console.WriteLine("=================================");
                Console.WriteLine("✅ SUPER ADMIN تم إنشاؤه!");
                Console.WriteLine($"📧 البريد: {superAdminEmail}");
                Console.WriteLine("🔑 كلمة المرور: SuperAdmin@2024");
                Console.WriteLine("=================================");
            }
        }

        // Create Regular Admin
        var adminEmail = "admin@bookstore.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Regular",
                LastName = "Admin",
                EmailConfirmed = true,
                Role = "Admin"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@2024");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("=================================");
                Console.WriteLine("✅ ADMIN عادي تم إنشاؤه!");
                Console.WriteLine($"📧 البريد: {adminEmail}");
                Console.WriteLine("🔑 كلمة المرور: Admin@2024");
                Console.WriteLine("=================================");
            }
        }

        // Add sample products if none exist
        if (!context.Products.Any())
        {
            var adminId = (await userManager.FindByEmailAsync(superAdminEmail))?.Id;

            context.Products.AddRange(
                new Product
                {
                    Title = "The Great Gatsby",
                    Author = "F. Scott Fitzgerald",
                    Description = "A classic novel about the American Dream",
                    Price = 12.99m,
                    StockQuantity = 100,
                    Status = "Active",
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow,
                    AddedByUserId = adminId
                },
                new Product
                {
                    Title = "To Kill a Mockingbird",
                    Author = "Harper Lee",
                    Description = "A novel about racial injustice",
                    Price = 14.99m,
                    StockQuantity = 80,
                    Status = "Active",
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow,
                    AddedByUserId = adminId
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
                    AddedByUserId = adminId
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ تم إضافة منتجات تجريبية.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ خطأ أثناء تهيئة البيانات: {ex.Message}");
    }
}

app.Run();