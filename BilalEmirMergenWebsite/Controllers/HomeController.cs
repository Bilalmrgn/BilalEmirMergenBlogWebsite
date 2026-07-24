using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
            // Record general site visit once per user session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("HasVisited")))
            {
                await _db.IncrementSiteVisitsAsync(Request.Path.Value ?? "/");
                HttpContext.Session.SetString("HasVisited", "True");
            }

            ViewBag.IsDemo = _db.IsDemo;

            var articles = await _db.GetArticlesAsync();
            var projects = await _db.GetProjectsAsync();

            var viewModel = new HomeViewModel
            {
                Articles = articles,
                Projects = projects
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

            // Increment read views
            await _db.IncrementArticleViewsAsync(slug);

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
    }
}
