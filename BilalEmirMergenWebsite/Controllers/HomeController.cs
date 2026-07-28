using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using BilalEmirMergenWebsite.Models;
using BilalEmirMergenWebsite.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace BilalEmirMergenWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDatabaseService _db;
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public HomeController(IDatabaseService db, ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _db = db;
            _logger = logger;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var swTotal = Stopwatch.StartNew();

            // Record general site visit once per user session in the background
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("HasVisited")))
            {
                var serviceProvider = HttpContext.RequestServices;
                var path = Request.Path.Value ?? "/";
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
                            await db.IncrementSiteVisitsAsync(path);
                        }
                    }
                    catch { }
                });
                HttpContext.Session.SetString("HasVisited", "True");
            }

            ViewBag.IsDemo = _db.IsDemo;

            // Measure Database query times
            var swDb = Stopwatch.StartNew();
            var articles = await _db.GetArticlesAsync();
            var projects = await _db.GetProjectsAsync();
            var socials = await _db.GetSocialsAsync();
            swDb.Stop();
            long dbMs = swDb.ElapsedMilliseconds;

            // Measure Model building times
            var swModel = Stopwatch.StartNew();
            var viewModel = new HomeViewModel
            {
                Articles = articles,
                Projects = projects,
                Socials = socials
            };
            swModel.Stop();
            long modelMs = swModel.ElapsedMilliseconds;

            swTotal.Stop();
            long totalMs = swTotal.ElapsedMilliseconds;

            // Log time details
            _logger.LogInformation("HomeController.Index - Veritabanı sorguları: {DbMs} ms | Model oluşturma: {ModelMs} ms | Toplam süre: {TotalMs} ms", dbMs, modelMs, totalMs);

            // Server-Timing Headers for Development
            if (_env.IsDevelopment())
            {
                Response.Headers["Server-Timing"] = $"database;dur={dbMs},external-api;dur=0,model-building;dur={modelMs},total;dur={totalMs}";
            }

            return View(viewModel);
        }

        [HttpGet("project/description/{id}")]
        public async Task<IActionResult> GetProjectDescription(string id)
        {
            var project = await _db.GetProjectByIdAsync(id);
            if (project == null) return NotFound();
            return Json(new { description = project.Description });
        }

        [Route("blog/{slug}")]
        public async Task<IActionResult> Article(string slug)
        {
            var article = await _db.GetArticleBySlugAsync(slug);
            if (article == null)
            {
                return NotFound();
            }

            // Increment read views in the background to avoid delaying article render
            var serviceProvider = HttpContext.RequestServices;
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
                        await db.IncrementArticleViewsAsync(slug);
                    }
                }
                catch { }
            });

            ViewBag.IsDemo = _db.IsDemo;
            return View(article);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class HomeViewModel
    {
        public List<Article> Articles { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public List<Social> Socials { get; set; } = new();
    }
}
