using BilalEmirMergenWebsite.Services;

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

// Configure Supabase or Fallback to Demo Mock Database Service
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];

bool isSupabaseConfigured = !string.IsNullOrEmpty(supabaseUrl) && 
                            supabaseUrl != "YOUR_SUPABASE_URL" && 
                            !string.IsNullOrEmpty(supabaseKey) && 
                            supabaseKey != "YOUR_SUPABASE_ANON_KEY";

if (isSupabaseConfigured)
{
    // Register real Supabase Client
    var options = new Supabase.SupabaseOptions
    {
        AutoConnectRealtime = false // Simple REST queries only
    };
    var supabaseClient = new Supabase.Client(supabaseUrl!, supabaseKey!, options);
    builder.Services.AddSingleton(supabaseClient);
    builder.Services.AddScoped<IDatabaseService, SupabaseDatabaseService>();
}
else
{
    // Register Demo Mock Database
    builder.Services.AddSingleton<IDatabaseService, MockDatabaseService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
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
