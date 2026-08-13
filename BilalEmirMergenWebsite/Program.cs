using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Middleware;
using BilalEmirMergenWebsite.Models;
using BilalEmirMergenWebsite.Services;
using BilalEmirMergenWebsite.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (!builder.Environment.IsDevelopment()) throw new InvalidOperationException("Jwt:Key must be configured in production.");
    jwtKey = "development-only-key-change-before-deployment-2026-bem-portfolio";
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Key"] = jwtKey });
}

builder.Services.AddControllersWithViews(options =>
{
    // Only fields explicitly marked as required should block an admin save.
    // Content entities intentionally contain many optional localized fields.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromHours(2); options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; options.Cookie.SameSite = SameSiteMode.Lax; });
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
        options.Cookie.Name = "bem.admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BilalEmirMergenWebsite",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BilalEmirMergenWebsite.Admin",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required.");
builder.Services.AddDbContext<AppDbContext>((services, options) => { options.UseSqlServer(connectionString); if (services.GetRequiredService<IWebHostEnvironment>().IsDevelopment()) options.EnableDetailedErrors(); });
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IImageService, ImageService>();

var app = builder.Build();
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            database.Database.Migrate();
        }
        catch (Exception exception)
        {
            app.Logger.LogError(exception, "Database migration failed. Existing content bootstrap will still be attempted.");
        }

        try
        {
            await PortfolioSeeder.SeedAsync(database);
            await EmbeddedImageOptimizer.OptimizeAsync(database, scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>(), app.Logger);
            var email = builder.Configuration["InitialAdmin:Email"];
            var password = builder.Configuration["InitialAdmin:Password"];
            var username = builder.Configuration["InitialAdmin:Username"] ?? "admin";
            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
            {
                var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
                var admin = database.AdminUsers.FirstOrDefault(user => user.Email == email);
                if (admin is null)
                {
                    admin = new AdminUser { Id = Guid.NewGuid().ToString(), Email = email };
                    database.AdminUsers.Add(admin);
                }
                admin.Username = username;
                admin.PasswordHash = passwordService.Hash(password);
                admin.Role = "Admin";
                database.SaveChanges();
            }
        }
        catch (Exception exception) { app.Logger.LogError(exception, "Database bootstrap failed. The app will continue so configuration can be corrected."); }
    }
}

app.UseMiddleware<ApiExceptionMiddleware>();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/error"); app.UseHsts(); }
else app.UseDeveloperExceptionPage();
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
        context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
        context.Response.Headers.TryAdd("Content-Security-Policy", "default-src 'self'; script-src 'self' https://unpkg.com; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src https://fonts.gstatic.com; img-src 'self' data: https:; connect-src 'self' https://api.iconify.design; frame-ancestors 'none'; base-uri 'self'; form-action 'self'");
        return Task.CompletedTask;
    });
    await next();
});
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "public,max-age=604800" });
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Root}/{id?}");
app.Run();

public partial class Program;
