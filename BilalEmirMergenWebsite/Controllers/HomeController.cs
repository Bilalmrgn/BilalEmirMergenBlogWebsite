using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using BilalEmirMergenWebsite.Models;
using BilalEmirMergenWebsite.Services;

namespace BilalEmirMergenWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDatabaseService _db;

        public HomeController(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
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

            // Fetch data in parallel
            var articlesTask = _db.GetArticlesAsync();
            var projectsTask = _db.GetProjectsAsync();
            var socialsTask = _db.GetSocialsAsync();

            await Task.WhenAll(articlesTask, projectsTask, socialsTask);

            var viewModel = new HomeViewModel
            {
                Articles = await articlesTask,
                Projects = await projectsTask,
                Socials = await socialsTask
            };

            return View(viewModel);
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
