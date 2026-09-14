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
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"];
var usesEphemeralJwtKey = false;
if (string.IsNullOrWhiteSpace(jwtKey))
{
    // The public portfolio and cookie-based admin UI must still be able to start
    // when the optional bearer API has not been configured yet. The generated
    // key is cryptographically strong, but tokens will not survive an app recycle.
    jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    usesEphemeralJwtKey = true;
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
// This deployment intentionally reads its database connection only from
// appsettings.json, as requested for the target IIS environment.
var appSettingsConfiguration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();
var connectionString = appSettingsConfiguration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is required in appsettings.json.");
}
builder.Services.AddDbContext<AppDbContext>((services, options) => { options.UseSqlServer(connectionString); if (services.GetRequiredService<IWebHostEnvironment>().IsDevelopment()) options.EnableDetailedErrors(); });
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddSingleton<IPortfolioCacheService, PortfolioCacheService>();
builder.Services.AddSingleton<IAnalyticsQueue, AnalyticsQueue>();
builder.Services.AddHostedService<AnalyticsBackgroundProcessor>();

var app = builder.Build();
if (usesEphemeralJwtKey)
{
    app.Logger.LogWarning("Jwt:Key is not configured. A temporary signing key was generated; bearer API tokens will be invalid after an application restart. Configure Jwt__Key in IIS.");
}

var runDatabaseStartupTasks = builder.Configuration.GetValue("Database:RunStartupTasks", builder.Environment.IsDevelopment());
if (!app.Environment.IsEnvironment("Testing") && runDatabaseStartupTasks)
{
    using (var scope = app.Services.CreateScope())
    {
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            var pendingMigrations = await database.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                app.Logger.LogInformation("Applying pending database migrations...");
                await database.Database.MigrateAsync();
            }
        }
        catch (Exception exception)
        {
            app.Logger.LogError(exception, "Database migration check failed. Existing content bootstrap will still be attempted.");
        }

        try
        {
            var seedDemoContent = builder.Configuration.GetValue<bool>("Database:SeedDemoContent");
            if (seedDemoContent)
            {
                await PortfolioSeeder.SeedAsync(database);
            }

            var optimizeImagesOnStartup = builder.Configuration.GetValue<bool>("Database:OptimizeImagesOnStartup", false);
            if (optimizeImagesOnStartup)
            {
                await EmbeddedImageOptimizer.OptimizeAsync(database, scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>(), app.Logger);
            }

            var email = builder.Configuration["InitialAdmin:Email"];
            var password = builder.Configuration["InitialAdmin:Password"];
            var username = builder.Configuration["InitialAdmin:Username"] ?? "admin";
            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
            {
                var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
                var admin = await database.AdminUsers.FirstOrDefaultAsync(user => user.Email == email);
                var requiresUpdate = false;

                if (admin is null)
                {
                    admin = new AdminUser { Id = Guid.NewGuid().ToString(), Email = email };
                    database.AdminUsers.Add(admin);
                    requiresUpdate = true;
                }

                if (admin.Username != username || admin.Role != "Admin" || !passwordService.Verify(password, admin.PasswordHash))
                {
                    admin.Username = username;
                    admin.PasswordHash = passwordService.Hash(password);
                    admin.Role = "Admin";
                    requiresUpdate = true;
                }

                if (requiresUpdate)
                {
                    await database.SaveChangesAsync();
                }
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
