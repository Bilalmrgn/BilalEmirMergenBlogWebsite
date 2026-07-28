using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Models;
using BilalEmirMergenWebsite.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support for Auth tracking
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure AppDbContext with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
    var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
    if (env.IsDevelopment())
    {
        options.EnableSensitiveDataLogging()
               .LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// Register SQL Server Database Service
builder.Services.AddScoped<IDatabaseService, SqlDatabaseService>();

var app = builder.Build();

// Automatically create the database, tables, and seed default data on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // dbContext.Database.EnsureCreated(); // Uzak SQL sunucusunda her açılışta şema kontrolü yapıp yavaşlamaya neden olduğu için kapatıldı.

        // Ensure the admin user exists and has the correct password hash
        string expectedHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Bilal.123")));
        var admin = dbContext.AdminUsers.FirstOrDefault(u => u.Email == "admin@bilal.com");
        if (admin == null)
        {
            dbContext.AdminUsers.Add(new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "admin@bilal.com",
                PasswordHash = expectedHash
            });
            dbContext.SaveChanges();
        }
        else if (admin.PasswordHash != expectedHash)
        {
            admin.PasswordHash = expectedHash;
            dbContext.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Veritabanı oluşturulurken hata oluştu: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
