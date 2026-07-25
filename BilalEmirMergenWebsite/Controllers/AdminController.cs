using Microsoft.AspNetCore.Mvc;
using BilalEmirMergenWebsite.Models;
using BilalEmirMergenWebsite.Services;

namespace BilalEmirMergenWebsite.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IDatabaseService _db;

        public AdminController(IDatabaseService db)
        {
            _db = db;
        }

        private bool IsAdminAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser"));
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            if (IsAdminAuthenticated())
            {
                return RedirectToAction("Dashboard");
            }
            ViewBag.IsDemo = _db.IsDemo;
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            ViewBag.IsDemo = _db.IsDemo;
            var success = await _db.SignInAsync(email, password);
            if (success)
            {
                HttpContext.Session.SetString("AdminUser", email);
                return RedirectToAction("Dashboard");
            }

            ModelState.AddModelError("", "E-posta veya şifre hatalı! (Demo giriş: admin@bilal.com / admin123)");
            return View();
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _db.SignOutAsync();
            HttpContext.Session.Remove("AdminUser");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard(string tab = "analytics")
        {
            if (!IsAdminAuthenticated())
            {
                return RedirectToAction("Login");
            }

            ViewBag.IsDemo = _db.IsDemo;
            ViewBag.ActiveTab = tab;
            ViewBag.UserEmail = HttpContext.Session.GetString("AdminUser");

            // Fetch data in parallel
            var articlesTask = _db.GetArticlesAsync();
            var projectsTask = _db.GetProjectsAsync();
            var socialsTask = _db.GetSocialsAsync();
            var siteVisitsTask = _db.GetSiteVisitsAsync();

            await Task.WhenAll(articlesTask, projectsTask, socialsTask, siteVisitsTask);

            var articles = await articlesTask;
            var projects = await projectsTask;
            var socials = await socialsTask;
            var siteVisits = await siteVisitsTask;

            // Calculate total views in memory from already retrieved articles list to eliminate a DB query
            var totalViews = articles.Sum(a => a.Views);

            var viewModel = new DashboardViewModel
            {
                Articles = articles,
                Projects = projects,
                Socials = socials,
                TotalViews = totalViews,
                SiteVisits = siteVisits,
                ArticlesCount = articles.Count
            };

            return View(viewModel);
        }

        [HttpPost("article/save")]
        public async Task<IActionResult> SaveArticle(string id, string title, string slug, string summary, string content, string coverImage)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login");

            var article = new Article
            {
                Title = title ?? string.Empty,
                Slug = slug ?? string.Empty,
                Summary = summary ?? string.Empty,
                Content = content ?? string.Empty,
                CoverImage = coverImage ?? string.Empty
            };

            if (string.IsNullOrEmpty(id))
            {
                await _db.AddArticleAsync(article);
            }
            else
            {
                await _db.UpdateArticleAsync(id, article);
            }

            return RedirectToAction("Dashboard", new { tab = "articles" });
        }

        [HttpPost("article/delete/{id}")]
        public async Task<IActionResult> DeleteArticle(string id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login");

            await _db.DeleteArticleAsync(id);
            return RedirectToAction("Dashboard", new { tab = "articles" });
        }

        [HttpPost("project/save")]
        public async Task<IActionResult> SaveProject(string id, string title, string description, string imageUrl, string projectUrl, string tags)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login");

            var tagsList = string.IsNullOrEmpty(tags) 
                ? new List<string>() 
                : tags.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();

            var project = new Project
            {
                Title = title ?? string.Empty,
                Description = description ?? string.Empty,
                ImageUrl = imageUrl ?? string.Empty,
                ProjectUrl = projectUrl ?? string.Empty,
                Tags = tagsList
            };

            if (string.IsNullOrEmpty(id))
            {
                await _db.AddProjectAsync(project);
            }
            else
            {
                await _db.UpdateProjectAsync(id, project);
            }

            return RedirectToAction("Dashboard", new { tab = "projects" });
        }

        [HttpPost("project/delete/{id}")]
        public async Task<IActionResult> DeleteProject(string id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login");

            await _db.DeleteProjectAsync(id);
            return RedirectToAction("Dashboard", new { tab = "projects" });
        }

        [HttpPost("social/save")]
        public async Task<IActionResult> SaveSocial(string id, string name, string icon, string url)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login");

            var social = new Social
            {
                Name = name ?? string.Empty,
                Icon = icon ?? "globe",
                Url = url ?? string.Empty
            };

            if (string.IsNullOrEmpty(id))
            {
                await _db.AddSocialAsync(social);
            }
            else
            {
                await _db.UpdateSocialAsync(id, social);
            }

            return RedirectToAction("Dashboard", new { tab = "socials" });
        }

        [HttpPost("social/delete/{id}")]
        public async Task<IActionResult> DeleteSocial(string id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login");

            await _db.DeleteSocialAsync(id);
            return RedirectToAction("Dashboard", new { tab = "socials" });
        }
    }

    public class DashboardViewModel
    {
        public List<Article> Articles { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public List<Social> Socials { get; set; } = new();
        public int TotalViews { get; set; }
        public int SiteVisits { get; set; }
        public int ArticlesCount { get; set; }
    }
}
