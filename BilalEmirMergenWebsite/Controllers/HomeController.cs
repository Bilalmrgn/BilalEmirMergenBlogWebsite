using System.Diagnostics;
using System.Text;
using System.Text.Json;
using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Models;
using BilalEmirMergenWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BilalEmirMergenWebsite.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly IPortfolioCacheService _cache;
    private readonly IAnalyticsQueue _analyticsQueue;

    public HomeController(AppDbContext context, IPortfolioCacheService cache, IAnalyticsQueue analyticsQueue)
    {
        _context = context;
        _cache = cache;
        _analyticsQueue = analyticsQueue;
    }

    [HttpGet("/")]
    [HttpGet("/{culture:regex(^(en|tr|ar)$)}")]
    public async Task<IActionResult> Index(string? culture = null)
    {
        var currentCulture = GetCurrentCulture(culture);
        SetCulture(currentCulture);

        if (HttpContext.Session.GetString("HasVisited") is null)
        {
            _analyticsQueue.Enqueue(new Analytics { Id = Guid.NewGuid().ToString(), EventType = "site_visit", PagePath = HttpContext.Request.Path.Value ?? "/", CreatedAt = DateTime.UtcNow });
            HttpContext.Session.SetString("HasVisited", "true");
        }

        const int homepageLimit = 10;

        var blogArticles = await _context.Articles.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Take(homepageLimit)
            .Select(x => new Article {
                Id = x.Id, Title = x.Title, TitleEn = x.TitleEn, TitleTr = x.TitleTr, TitleAr = x.TitleAr,
                Slug = x.Slug, CategoryEn = x.CategoryEn, CategoryTr = x.CategoryTr, CategoryAr = x.CategoryAr,
                Summary = x.Summary, SummaryEn = x.SummaryEn, SummaryTr = x.SummaryTr, SummaryAr = x.SummaryAr,
                CoverImage = x.CoverImage, ExternalUrl = x.ExternalUrl, Tags = x.Tags, ContentLanguage = x.ContentLanguage,
                IsFeatured = x.IsFeatured, IsInternal = x.IsInternal, IsActive = x.IsActive, SortOrder = x.SortOrder,
                Views = x.Views, CreatedAt = x.CreatedAt, PublishedAt = x.PublishedAt
            }).ToListAsync();

        var certs = await _context.Certificates.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Take(homepageLimit)
            .Select(x => new Certificate {
                Id = x.Id, NameEn = x.NameEn, NameTr = x.NameTr, NameAr = x.NameAr, Organization = x.Organization,
                IssueDate = x.IssueDate, ExpirationDate = x.ExpirationDate, DoesNotExpire = x.DoesNotExpire,
                CredentialId = x.CredentialId, CredentialUrl = x.CredentialUrl, DescriptionEn = x.DescriptionEn,
                DescriptionTr = x.DescriptionTr, DescriptionAr = x.DescriptionAr, CertificateImage = x.CertificateImage,
                OrganizationLogo = x.OrganizationLogo, SortOrder = x.SortOrder, IsActive = x.IsActive
            }).ToListAsync();

        var projects = await _context.Projects.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.SortOrder)
            .ThenByDescending(x => x.CreatedAt)
            .Take(homepageLimit)
            .Select(x => new Project {
                Id = x.Id, Title = x.Title, TitleEn = x.TitleEn, TitleTr = x.TitleTr, Slug = x.Slug,
                CategoryEn = x.CategoryEn, Status = x.Status, ShortDescriptionEn = x.ShortDescriptionEn,
                ShortDescriptionTr = x.ShortDescriptionTr, ImageUrl = x.ImageUrl, Tags = x.Tags,
                GitHubUrl = x.GitHubUrl, ProjectUrl = x.ProjectUrl, IsFeatured = x.IsFeatured,
                IsActive = x.IsActive, SortOrder = x.SortOrder, CreatedAt = x.CreatedAt
            }).ToListAsync();

        var settings = await _cache.GetSiteSettingsAsync(_context);
        var socials = await _cache.GetSocialsAsync(_context);

        var model = new HomeViewModel
        {
            Culture = currentCulture,
            Settings = settings,
            AboutSections = await _cache.GetAboutSectionsAsync(_context),
            Services = await _cache.GetServicesAsync(_context),
            Certificates = certs,
            CertPage = 1,
            CertTotalPages = 1,
            CertTotalCount = await _context.Certificates.CountAsync(x => x.IsActive),
            Experiences = await _cache.GetExperiencesAsync(_context),
            Educations = await _cache.GetEducationsAsync(_context),
            SkillCategories = await _cache.GetSkillCategoriesAsync(_context),
            Concepts = await _cache.GetEngineeringConceptsAsync(_context),
            Languages = await _cache.GetSpokenLanguagesAsync(_context),
            AcademicFoundations = await _cache.GetAcademicFoundationsAsync(_context),
            Projects = projects,
            Articles = blogArticles,
            BlogPage = 1,
            BlogTotalPages = 1,
            BlogTotalCount = await _context.Articles.CountAsync(x => x.IsActive),
            Socials = socials
        };

        ApplySeo(model.Settings, currentCulture);
        ViewData["JsonLd"] = JsonSerializer.Serialize(new Dictionary<string, object?> { ["@context"] = "https://schema.org", ["@type"] = "Person", ["name"] = model.Settings.FullName, ["jobTitle"] = model.Settings.TitleEn, ["url"] = model.Settings.CanonicalBaseUrl.TrimEnd('/'), ["email"] = model.Settings.Email, ["homeLocation"] = new Dictionary<string, object?> { ["@type"] = "Place", ["name"] = model.Settings.LocationEn }, ["sameAs"] = model.Socials.Where(item => item.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(item => item.Url).ToArray() });
        return View(model);
    }

    [HttpGet("/projects/{slug}")]
    [HttpGet("/{culture:regex(^(en|tr|ar)$)}/projects/{slug}")]
    public async Task<IActionResult> Project(string slug, string? culture = null)
    {
        var currentCulture = GetCurrentCulture(culture);
        SetCulture(currentCulture);
        var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);
        if (project is null) return NotFound();
        var settings = await _cache.GetSiteSettingsAsync(_context);
        ViewBag.Culture = currentCulture;
        ViewData["Title"] = TextExtensions.Localized(project.TitleEn, project.TitleTr, project.TitleAr, currentCulture).Or(project.Title);
        ViewData["Description"] = TextExtensions.Localized(project.ShortDescriptionEn, project.ShortDescriptionTr, project.ShortDescriptionAr, currentCulture);
        ViewData["Canonical"] = $"{settings.CanonicalBaseUrl.TrimEnd('/')}/projects/{project.Slug}";
        ViewData["OgImage"] = project.ImageUrl.Or(settings.OpenGraphImage);
        ViewData["JsonLd"] = JsonSerializer.Serialize(new Dictionary<string, object?> { ["@context"] = "https://schema.org", ["@type"] = "CreativeWork", ["name"] = project.TitleEn.Or(project.Title), ["description"] = project.ShortDescriptionEn, ["url"] = ViewData["Canonical"], ["image"] = project.ImageUrl, ["keywords"] = project.Tags });
        return View(project);
    }

    [HttpGet("/blogs")]
    [HttpGet("/blog-list")]
    [HttpGet("/{culture:regex(^(en|tr|ar)$)}/blogs")]
    [HttpGet("/{culture:regex(^(en|tr|ar)$)}/blog-list")]
    public async Task<IActionResult> BlogList(int page = 1, string? culture = null)
    {
        var currentCulture = GetCurrentCulture(culture);
        SetCulture(currentCulture);

        if (page < 1) page = 1;
        const int pageSize = 10;

        var blogQuery = _context.Articles.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt);

        var totalArticles = await blogQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var pagedArticles = await blogQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Article
            {
                Id = x.Id, Title = x.Title, TitleTr = x.TitleTr, TitleEn = x.TitleEn, Slug = x.Slug,
                Summary = x.Summary, SummaryTr = x.SummaryTr, SummaryEn = x.SummaryEn,
                CategoryTr = x.CategoryTr, CategoryEn = x.CategoryEn, CoverImage = x.CoverImage,
                IsInternal = x.IsInternal, ExternalUrl = x.ExternalUrl, PublishedAt = x.PublishedAt,
                CreatedAt = x.CreatedAt, Content = x.Content, ContentTr = x.ContentTr, ContentEn = x.ContentEn
            })
            .ToListAsync();

        var settings = await _cache.GetSiteSettingsAsync(_context);

        var viewModel = new BlogListViewModel
        {
            Articles = pagedArticles, CurrentPage = page, TotalPages = totalPages,
            TotalCount = totalArticles, PageSize = pageSize, Culture = currentCulture,
            IsTr = currentCulture == "tr", Settings = settings
        };

        ViewData["Title"] = viewModel.IsTr ? "Tüm Blog Yazıları — Bilal Emir Mergen" : "All Blog Posts — Bilal Emir Mergen";
        ViewData["Description"] = viewModel.IsTr ? "Yazılım mühendisliği ve teknoloji üzerine tüm içerikler." : "All articles on software engineering and technology.";

        return View("BlogList", viewModel);
    }

    [HttpGet("/projects-list")]
    [HttpGet("/all-projects")]
    [HttpGet("/{culture:regex(^(en|tr|ar)$)}/projects-list")]
    [HttpGet("/{culture:regex(^(en|tr|ar)$)}/all-projects")]
    public async Task<IActionResult> ProjectList(int page = 1, string? culture = null)
    {
        var currentCulture = GetCurrentCulture(culture);
        SetCulture(currentCulture);

        if (page < 1) page = 1;
        const int pageSize = 10;

        var projectQuery = _context.Projects.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.SortOrder)
            .ThenByDescending(x => x.CreatedAt);

        var totalProjects = await projectQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalProjects / (double)pageSize);
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var pagedProjects = await projectQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Project
            {
                Id = x.Id, Title = x.Title, TitleEn = x.TitleEn, TitleTr = x.TitleTr, Slug = x.Slug,
                CategoryEn = x.CategoryEn, Status = x.Status, ShortDescriptionEn = x.ShortDescriptionEn,
                ShortDescriptionTr = x.ShortDescriptionTr, DescriptionEn = x.DescriptionEn,
                DescriptionTr = x.DescriptionTr, ImageUrl = x.ImageUrl, Tags = x.Tags,
                GitHubUrl = x.GitHubUrl, ProjectUrl = x.ProjectUrl, IsFeatured = x.IsFeatured,
                IsActive = x.IsActive, SortOrder = x.SortOrder, CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        var settings = await _cache.GetSiteSettingsAsync(_context);

        var viewModel = new ProjectListViewModel
        {
            Projects = pagedProjects, CurrentPage = page, TotalPages = totalPages,
            TotalCount = totalProjects, PageSize = pageSize, Culture = currentCulture,
            IsTr = currentCulture == "tr", Settings = settings
        };

        ViewData["Title"] = viewModel.IsTr ? "Tüm Projelerim — Bilal Emir Mergen" : "All Projects — Bilal Emir Mergen";
        ViewData["Description"] = viewModel.IsTr ? "Geliştirdiğim tüm yazılım projeleri ve vaka incelemeleri." : "All software engineering projects and case studies.";

        return View("ProjectList", viewModel);
    }

    [HttpGet("/certificates-list")]
    [HttpGet("/all-certificates")]
    [HttpGet("/{culture:regex(^(en|tr|ar)$)}/certificates-list")]
    [HttpGet("/{culture:regex(^(en|tr|ar)$)}/all-certificates")]
    public async Task<IActionResult> CertificateList(int page = 1, string? culture = null)
    {
        var currentCulture = GetCurrentCulture(culture);
        SetCulture(currentCulture);

        if (page < 1) page = 1;
        const int pageSize = 10;

        var certQuery = _context.Certificates.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder);

        var totalCertificates = await certQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCertificates / (double)pageSize);
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var pagedCertificates = await certQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Certificate
            {
                Id = x.Id, NameEn = x.NameEn, NameTr = x.NameTr, NameAr = x.NameAr, Organization = x.Organization,
                IssueDate = x.IssueDate, ExpirationDate = x.ExpirationDate, DoesNotExpire = x.DoesNotExpire,
                CredentialId = x.CredentialId, CredentialUrl = x.CredentialUrl, DescriptionEn = x.DescriptionEn,
                DescriptionTr = x.DescriptionTr, DescriptionAr = x.DescriptionAr, CertificateImage = x.CertificateImage,
                OrganizationLogo = x.OrganizationLogo, SortOrder = x.SortOrder, IsActive = x.IsActive
            })
            .ToListAsync();

        var settings = await _cache.GetSiteSettingsAsync(_context);

        var viewModel = new CertificateListViewModel
        {
            Certificates = pagedCertificates, CurrentPage = page, TotalPages = totalPages,
            TotalCount = totalCertificates, PageSize = pageSize, Culture = currentCulture,
            IsTr = currentCulture == "tr", Settings = settings
        };

        ViewData["Title"] = viewModel.IsTr ? "Tüm Sertifikalarım — Bilal Emir Mergen" : "All Certificates — Bilal Emir Mergen";
        ViewData["Description"] = viewModel.IsTr ? "Kazandığım tüm uluslararası sertifikalar ve başarı belgeleri." : "All professional credentials and certifications.";

        return View("CertificateList", viewModel);
    }

    [HttpGet("/blog/{slug}")]
    [HttpGet("/{culture:regex(^(en|tr|ar)$)}/blog/{slug}")]
    public async Task<IActionResult> Article(string slug, string? culture = null)
    {
        var article = await _context.Articles.FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);
        if (article is null) return NotFound();

        var currentCulture = GetCurrentCulture(culture ?? article.ContentLanguage);
        SetCulture(currentCulture);

        await _context.Articles
            .Where(x => x.Id == article.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Views, a => a.Views + 1));
        article.Views++;

        var settings = await _cache.GetSiteSettingsAsync(_context);
        var ordered = await _context.Articles.AsNoTracking()
            .Where(x => x.IsActive && x.Id != article.Id)
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Select(x => new Article { Id = x.Id, Slug = x.Slug, Title = x.Title, TitleEn = x.TitleEn, TitleTr = x.TitleTr, PublishedAt = x.PublishedAt, CreatedAt = x.CreatedAt })
            .ToListAsync();

        var currentDate = article.PublishedAt ?? article.CreatedAt;
        ViewBag.Culture = currentCulture;
        ViewData["Title"] = article.SeoTitle.Or(article.TitleEn.Or(article.Title));
        ViewData["Description"] = article.SeoDescription.Or(article.SummaryEn.Or(article.Summary));
        ViewData["Canonical"] = $"{settings.CanonicalBaseUrl.TrimEnd('/')}/blog/{article.Slug}";
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
        var urls = new List<string> { root };
        var projects = await _context.Projects.AsNoTracking().Where(x => x.IsActive && x.Slug != "").Select(x => x.Slug).ToListAsync();
        var articles = await _context.Articles.AsNoTracking().Where(x => x.IsActive).Select(x => x.Slug).ToListAsync();
        urls.AddRange(projects.Select(slug => $"{root}/projects/{slug}"));
        urls.AddRange(articles.Select(slug => $"{root}/blog/{slug}"));
        var xml = "<?xml \"version=1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">" + string.Join("", urls.Select(x => $"<url><loc>{System.Net.WebUtility.HtmlEncode(x)}</loc></url>")) + "</urlset>";
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

    private string GetCurrentCulture(string? cultureFromRoute = null)
    {
        if (!string.IsNullOrWhiteSpace(cultureFromRoute) && (cultureFromRoute == "en" || cultureFromRoute == "tr" || cultureFromRoute == "ar"))
        {
            return cultureFromRoute;
        }
        var cookieCulture = Request.Cookies["portfolio-language"];
        if (!string.IsNullOrWhiteSpace(cookieCulture) && (cookieCulture == "en" || cookieCulture == "tr" || cookieCulture == "ar"))
        {
            return cookieCulture;
        }
        return "tr";
    }

    private void SetCulture(string culture)
    {
        Response.Cookies.Append("portfolio-language", culture, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax });
        ViewBag.Culture = culture;
    }

    private void ApplySeo(SiteSettings settings, string culture)
    {
        ViewData["Title"] = TextExtensions.Localized(settings.SeoTitleEn, settings.SeoTitleTr, settings.SeoTitleAr, culture).Or(culture == "tr" ? "BEM.dev | Yazılım Geliştirici Portfolyosu" : "BEM.dev | Software Developer Portfolio");
        ViewData["Description"] = TextExtensions.Localized(settings.SeoDescriptionEn, settings.SeoDescriptionTr, settings.SeoDescriptionAr, culture).Or(culture == "tr" ? "Projeler, teknik yazılar ve yazılım geliştirme çalışmaları." : "Projects, technical writing and software development work.");
        ViewData["Canonical"] = $"{settings.CanonicalBaseUrl.TrimEnd('/')}";
        ViewData["OgImage"] = settings.OpenGraphImage;
    }
}

public class HomeViewModel
{
    public string Culture { get; set; } = "tr";
    public bool IsTr => Culture == "tr";
    public bool IsAr => Culture == "ar";
    public SiteSettings Settings { get; set; } = new();
    public List<Experience> Experiences { get; set; } = new();
    public List<AboutSection> AboutSections { get; set; } = new();
    public List<Service> Services { get; set; } = new();
    public List<Certificate> Certificates { get; set; } = new();
    public int CertPage { get; set; } = 1;
    public int CertTotalPages { get; set; } = 1;
    public int CertTotalCount { get; set; } = 0;
    public List<Education> Educations { get; set; } = new();
    public List<SkillCategory> SkillCategories { get; set; } = new();
    public List<EngineeringConcept> Concepts { get; set; } = new();
    public List<SpokenLanguage> Languages { get; set; } = new();
    public List<AcademicFoundation> AcademicFoundations { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
    public List<Article> Articles { get; set; } = new();
    public int BlogPage { get; set; } = 1;
    public int BlogTotalPages { get; set; } = 1;
    public int BlogTotalCount { get; set; } = 0;
    public bool IsShowAllBlog { get; set; }
    public List<Social> Socials { get; set; } = new();
}

public class ArticleDetailViewModel
{
    public Article Article { get; set; } = new();
    public Article? Previous { get; set; }
    public Article? Next { get; set; }
}

public class BlogListViewModel
{
    public List<Article> Articles { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string Culture { get; set; } = "tr";
    public bool IsTr { get; set; } = true;
    public SiteSettings Settings { get; set; } = new();
}

public class ProjectListViewModel
{
    public List<Project> Projects { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string Culture { get; set; } = "tr";
    public bool IsTr { get; set; } = true;
    public SiteSettings Settings { get; set; } = new();
}

public class CertificateListViewModel
{
    public List<Certificate> Certificates { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string Culture { get; set; } = "tr";
    public bool IsTr { get; set; } = true;
    public SiteSettings Settings { get; set; } = new();
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

    public static string FormatBlogMeta(DateTime? date, string? contentHtml, string culture)
    {
        var dt = date ?? DateTime.UtcNow;
        var cleanText = System.Text.RegularExpressions.Regex.Replace(contentHtml ?? string.Empty, "<[^>]+>", " ");
        var words = cleanText.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var readTime = Math.Max(1, (int)Math.Ceiling(words / 220.0));

        if (culture == "tr")
        {
            var trCulture = new System.Globalization.CultureInfo("tr-TR");
            return $"{dt.ToString("d MMMM yyyy", trCulture)} · {readTime} dk okuma";
        }
        else
        {
            var enCulture = new System.Globalization.CultureInfo("en-US");
            return $"{dt.ToString("MMM d, yyyy", enCulture)} · {readTime} min read";
        }
    }
}
