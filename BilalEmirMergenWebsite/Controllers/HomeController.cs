using System.Diagnostics;
using System.Text;
using System.Text.Json;
using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BilalEmirMergenWebsite.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    public HomeController(AppDbContext context) => _context = context;

    [HttpGet("/")]
    public IActionResult Root() => RedirectToAction(nameof(Index), new { culture = "en" });

    [HttpGet("/{culture:regex(^(en|tr|ar)$)}")]
    public async Task<IActionResult> Index(string culture = "en")
    {
        if (culture != "en") return RedirectToAction(nameof(Index), new { culture = "en" });
        SetCulture(culture);
        if (HttpContext.Session.GetString("HasVisited") is null)
        {
            _context.Analytics.Add(new Analytics { Id = Guid.NewGuid().ToString(), EventType = "site_visit", PagePath = $"/{culture}", CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("HasVisited", "true");
        }

        var model = new HomeViewModel
        {
            Culture = culture,
            Settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SiteSettings(),
            AboutSections = await _context.AboutSections.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            Services = await _context.Services.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            Certificates = await _context.Certificates.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            Experiences = await _context.Experiences.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            Educations = await _context.Educations.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            SkillCategories = await _context.SkillCategories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).Include(x => x.Skills.Where(s => s.IsActive).OrderBy(s => s.SortOrder)).ToListAsync(),
            Concepts = await _context.EngineeringConcepts.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            Languages = await _context.SpokenLanguages.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            AcademicFoundations = await _context.AcademicFoundations.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            Projects = await _context.Projects.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.IsFeatured).ThenBy(x => x.SortOrder).ThenByDescending(x => x.CreatedAt).ToListAsync(),
            Articles = await _context.Articles.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.IsFeatured).ThenBy(x => x.SortOrder).ThenByDescending(x => x.PublishedAt ?? x.CreatedAt).Take(3).Select(x => new Article { Id=x.Id,Title=x.Title,TitleEn=x.TitleEn,TitleTr=x.TitleTr,TitleAr=x.TitleAr,Slug=x.Slug,CategoryEn=x.CategoryEn,CategoryTr=x.CategoryTr,CategoryAr=x.CategoryAr,Summary=x.Summary,SummaryEn=x.SummaryEn,SummaryTr=x.SummaryTr,SummaryAr=x.SummaryAr,CoverImage=x.CoverImage,ExternalUrl=x.ExternalUrl,Tags=x.Tags,ContentLanguage=x.ContentLanguage,IsFeatured=x.IsFeatured,IsInternal=x.IsInternal,IsActive=x.IsActive,SortOrder=x.SortOrder,Views=x.Views,CreatedAt=x.CreatedAt,PublishedAt=x.PublishedAt }).ToListAsync(),
            Socials = await _context.Socials.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync()
        };
        ApplySeo(model.Settings, culture);
        ViewData["JsonLd"] = JsonSerializer.Serialize(new Dictionary<string, object?> { ["@context"] = "https://schema.org", ["@type"] = "Person", ["name"] = model.Settings.FullName, ["jobTitle"] = model.Settings.TitleEn, ["url"] = $"{model.Settings.CanonicalBaseUrl.TrimEnd('/')}/en", ["email"] = model.Settings.Email, ["homeLocation"] = new Dictionary<string, object?> { ["@type"] = "Place", ["name"] = model.Settings.LocationEn }, ["sameAs"] = model.Socials.Where(item => item.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(item => item.Url).ToArray() });
        return View(model);
    }

    [HttpGet("/{culture:regex(^(en|tr|ar)$)}/projects/{slug}")]
    public async Task<IActionResult> Project(string culture, string slug)
    {
        if (culture != "en") return RedirectToAction(nameof(Project), new { culture = "en", slug });
        SetCulture(culture);
        var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);
        if (project is null) return NotFound();
        var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SiteSettings();
        ViewBag.Culture = culture;
        ViewData["Title"] = TextExtensions.Localized(project.TitleEn, project.TitleTr, project.TitleAr, culture).Or(project.Title);
        ViewData["Description"] = TextExtensions.Localized(project.ShortDescriptionEn, project.ShortDescriptionTr, project.ShortDescriptionAr, culture);
        ViewData["Canonical"] = $"{settings.CanonicalBaseUrl.TrimEnd('/')}/en/projects/{project.Slug}";
        ViewData["OgImage"] = project.ImageUrl.Or(settings.OpenGraphImage);
        ViewData["JsonLd"] = JsonSerializer.Serialize(new Dictionary<string, object?> { ["@context"] = "https://schema.org", ["@type"] = "CreativeWork", ["name"] = project.TitleEn.Or(project.Title), ["description"] = project.ShortDescriptionEn, ["url"] = ViewData["Canonical"], ["image"] = project.ImageUrl, ["keywords"] = project.Tags });
        return View(project);
    }

    [HttpGet("/{culture:regex(^(en|tr|ar)$)}/blog/{slug}")]
    public async Task<IActionResult> Article(string culture, string slug)
    {
        if (culture != "en") return RedirectToAction(nameof(Article), new { culture = "en", slug });
        SetCulture(culture);
        var article = await _context.Articles.FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive && (!x.PublishedAt.HasValue || x.PublishedAt <= DateTime.UtcNow));
        if (article is null) return NotFound();
        article.Views++;
        await _context.SaveChangesAsync();
        var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SiteSettings();
        var ordered = await _context.Articles.AsNoTracking().Where(x => x.IsActive && x.Id != article.Id && (!x.PublishedAt.HasValue || x.PublishedAt <= DateTime.UtcNow)).OrderByDescending(x => x.PublishedAt ?? x.CreatedAt).Select(x => new Article { Id = x.Id, Slug = x.Slug, Title = x.Title, TitleEn = x.TitleEn, PublishedAt = x.PublishedAt, CreatedAt = x.CreatedAt }).ToListAsync();
        var currentDate = article.PublishedAt ?? article.CreatedAt;
        ViewBag.Culture = culture;
        ViewData["Title"] = article.SeoTitle.Or(article.TitleEn.Or(article.Title));
        ViewData["Description"] = article.SeoDescription.Or(article.SummaryEn.Or(article.Summary));
        ViewData["Canonical"] = $"{settings.CanonicalBaseUrl.TrimEnd('/')}/en/blog/{article.Slug}";
        ViewData["OgImage"] = article.CoverImage.Or(settings.OpenGraphImage);
        ViewData["OgType"] = "article";
        ViewData["JsonLd"] = JsonSerializer.Serialize(new Dictionary<string, object?> { ["@context"] = "https://schema.org", ["@type"] = "BlogPosting", ["headline"] = article.TitleEn.Or(article.Title), ["description"] = article.SummaryEn.Or(article.Summary), ["image"] = article.CoverImage, ["datePublished"] = (article.PublishedAt ?? article.CreatedAt).ToString("O"), ["dateModified"] = article.UpdatedAt.ToString("O"), ["author"] = new Dictionary<string, object?> { ["@type"] = "Person", ["name"] = settings.FullName }, ["mainEntityOfPage"] = ViewData["Canonical"] });
        return View(new ArticleDetailViewModel { Article = article, Previous = ordered.Where(item => (item.PublishedAt ?? item.CreatedAt) < currentDate).OrderByDescending(item => item.PublishedAt ?? item.CreatedAt).FirstOrDefault(), Next = ordered.Where(item => (item.PublishedAt ?? item.CreatedAt) > currentDate).OrderBy(item => item.PublishedAt ?? item.CreatedAt).FirstOrDefault() });
    }

    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SiteSettings();
        var root = settings.CanonicalBaseUrl.TrimEnd('/');
        var urls = new List<string> { $"{root}/en" };
        var projects = await _context.Projects.AsNoTracking().Where(x => x.IsActive && x.Slug != "").Select(x => x.Slug).ToListAsync();
        var articles = await _context.Articles.AsNoTracking().Where(x => x.IsActive).Select(x => x.Slug).ToListAsync();
        urls.AddRange(projects.Select(slug => $"{root}/en/projects/{slug}"));
        urls.AddRange(articles.Select(slug => $"{root}/en/blog/{slug}"));
        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">" + string.Join("", urls.Select(x => $"<url><loc>{System.Net.WebUtility.HtmlEncode(x)}</loc></url>")) + "</urlset>";
        return Content(xml, "application/xml", Encoding.UTF8);
    }

    [HttpGet("robots.txt")]
    public async Task<IActionResult> Robots()
    {
        var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SiteSettings();
        return Content($"User-agent: *\nAllow: /\nDisallow: /admin\nSitemap: {settings.CanonicalBaseUrl.TrimEnd('/')}/sitemap.xml\n", "text/plain");
    }

    [HttpGet("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    private void SetCulture(string culture)
    {
        Response.Cookies.Append("portfolio-language", culture, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax });
        ViewBag.Culture = culture;
    }

    private void ApplySeo(SiteSettings settings, string culture)
    {
        ViewData["Title"] = TextExtensions.Localized(settings.SeoTitleEn, settings.SeoTitleTr, settings.SeoTitleAr, culture).Or(culture == "tr" ? "BEM.dev | Yazılım Geliştirici Portfolyosu" : culture == "ar" ? "BEM.dev | ملف مطور برمجيات" : "BEM.dev | Software Developer Portfolio");
        ViewData["Description"] = TextExtensions.Localized(settings.SeoDescriptionEn, settings.SeoDescriptionTr, settings.SeoDescriptionAr, culture).Or(culture == "tr" ? "Projeler, teknik yazılar ve yazılım geliştirme çalışmaları." : culture == "ar" ? "مشاريع ومقالات تقنية وأعمال تطوير البرمجيات." : "Projects, technical writing and software development work.");
        ViewData["Canonical"] = $"{settings.CanonicalBaseUrl.TrimEnd('/')}/{culture}";
        ViewData["OgImage"] = settings.OpenGraphImage;
        ViewData["Title"] = settings.SeoTitleEn.Or("Bilal Emir Mergen — Full Stack Software Engineer");
        ViewData["Description"] = settings.SeoDescriptionEn.Or("Selected projects, technical writing and software development work by Bilal Emir Mergen.");
        ViewData["Canonical"] = $"{settings.CanonicalBaseUrl.TrimEnd('/')}/en";
    }
}

public class HomeViewModel
{
    public string Culture { get; set; } = "en";
    public bool IsTr => Culture == "tr";
    public bool IsAr => Culture == "ar";
    public SiteSettings Settings { get; set; } = new();
    public List<Experience> Experiences { get; set; } = new();
    public List<AboutSection> AboutSections { get; set; } = new();
    public List<Service> Services { get; set; } = new();
    public List<Certificate> Certificates { get; set; } = new();
    public List<Education> Educations { get; set; } = new();
    public List<SkillCategory> SkillCategories { get; set; } = new();
    public List<EngineeringConcept> Concepts { get; set; } = new();
    public List<SpokenLanguage> Languages { get; set; } = new();
    public List<AcademicFoundation> AcademicFoundations { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
    public List<Article> Articles { get; set; } = new();
    public List<Social> Socials { get; set; } = new();
}

public class ArticleDetailViewModel
{
    public Article Article { get; set; } = new();
    public Article? Previous { get; set; }
    public Article? Next { get; set; }
}

public static class TextExtensions
{
    public static string Or(this string? value, string? fallback) => string.IsNullOrWhiteSpace(value) ? fallback ?? string.Empty : value;
    public static string Localized(string? en, string? tr, string? ar, string culture) => culture switch { "tr" => tr.Or(en.Or(ar)), "ar" => ar.Or(en.Or(tr)), _ => en.Or(tr.Or(ar)) };
    public static string PlainSummary(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains("base64", StringComparison.OrdinalIgnoreCase)) return string.Empty;
        var clean = System.Text.RegularExpressions.Regex.Replace(value, "<[^>]*>", " ");
        clean = System.Text.RegularExpressions.Regex.Replace(clean, "<[^>]*$", " ");
        return System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(clean, "\\s+", " ")).Trim();
    }
}
