using System.Text.RegularExpressions;
using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Dtos;
using BilalEmirMergenWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BilalEmirMergenWebsite.Controllers;

[ApiController]
[Route("api")]
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
public sealed class PortfolioApiController(AppDbContext database) : ControllerBase
{
    [HttpGet("about")]
    public async Task<ActionResult<AboutDto>> About(CancellationToken cancellationToken)
    {
        var settings = await database.SiteSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken) ?? new SiteSettings();
        var about = await database.AboutSections.AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.SortOrder).FirstOrDefaultAsync(cancellationToken) ?? new AboutSection();
        return new AboutDto(about.EyebrowEn, about.TitleEn, about.ShortTextEn, about.DescriptionEn, about.ImageUrl.Or(settings.ProfileImage), settings.CvEnglishFile, settings.LocationEn, settings.Email, settings.IsAvailable, settings.AvailabilityEn);
    }

    [HttpGet("experiences")]
    public async Task<ActionResult<IReadOnlyList<ExperienceDto>>> Experiences(CancellationToken cancellationToken) =>
        (await database.Experiences.AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.SortOrder).ToListAsync(cancellationToken)).Select(Map).ToList();

    [HttpGet("educations")]
    public async Task<ActionResult<IReadOnlyList<EducationDto>>> Educations(CancellationToken cancellationToken) =>
        (await database.Educations.AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.SortOrder).ToListAsync(cancellationToken)).Select(Map).ToList();

    [HttpGet("skill-categories")]
    [HttpGet("skills")]
    public async Task<ActionResult<IReadOnlyList<SkillCategoryDto>>> Skills(CancellationToken cancellationToken) =>
        (await database.SkillCategories.AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.SortOrder).Include(item => item.Skills.Where(skill => skill.IsActive).OrderBy(skill => skill.SortOrder)).ToListAsync(cancellationToken)).Select(Map).ToList();

    [HttpGet("projects")]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> Projects([FromQuery] string? category, CancellationToken cancellationToken)
    {
        var query = database.Projects.AsNoTracking().Where(item => item.IsActive);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(item => item.CategoryEn == category);
        return (await query.OrderBy(item => item.SortOrder).ThenByDescending(item => item.CreatedAt).ToListAsync(cancellationToken)).Select(Map).ToList();
    }

    [HttpGet("projects/{slug}")]
    public async Task<ActionResult<ProjectDto>> Project(string slug, CancellationToken cancellationToken)
    {
        var project = await database.Projects.AsNoTracking().FirstOrDefaultAsync(item => item.Slug == slug && item.IsActive, cancellationToken);
        return project is null ? NotFound() : Map(project);
    }

    [HttpGet("languages")]
    public async Task<ActionResult<IReadOnlyList<LanguageDto>>> Languages(CancellationToken cancellationToken) =>
        (await database.SpokenLanguages.AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.SortOrder).ToListAsync(cancellationToken)).Select(Map).ToList();

    [HttpGet("blog")]
    public async Task<ActionResult<IReadOnlyList<BlogPostDto>>> Blog(CancellationToken cancellationToken) =>
        (await database.Articles.AsNoTracking().Where(item => item.IsActive && (!item.PublishedAt.HasValue || item.PublishedAt <= DateTime.UtcNow)).OrderByDescending(item => item.PublishedAt ?? item.CreatedAt).ToListAsync(cancellationToken)).Select(Map).ToList();

    [HttpGet("blog/{slug}")]
    public async Task<ActionResult<BlogPostDto>> BlogPost(string slug, CancellationToken cancellationToken)
    {
        var article = await database.Articles.AsNoTracking().FirstOrDefaultAsync(item => item.Slug == slug && item.IsActive && (!item.PublishedAt.HasValue || item.PublishedAt <= DateTime.UtcNow), cancellationToken);
        return article is null ? NotFound() : Map(article);
    }

    [HttpGet("social-links")]
    public async Task<ActionResult<IReadOnlyList<SocialLinkDto>>> SocialLinks(CancellationToken cancellationToken) =>
        (await database.Socials.AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.SortOrder).ToListAsync(cancellationToken)).Select(item => new SocialLinkDto(item.Id, item.Name, item.Icon, item.Url, item.SortOrder)).ToList();

    private static ExperienceDto Map(Experience item) => new(item.Id, item.RoleEn.Or(item.RoleTr), item.CompanyName, item.LocationEn.Or(item.LocationTr), item.StartDate, item.EndDate, item.IsCurrent, item.DescriptionEn.Or(item.DescriptionTr), Lines(item.BulletPointsEn.Or(item.BulletPointsTr)), Csv(item.Technologies), item.CompanyLogoUrl, item.Icon, item.TimelineColor, item.SortOrder);
    private static EducationDto Map(Education item) => new(item.Id, item.SchoolName, item.DegreeEn.Or(item.DegreeTr), item.FieldEn.Or(item.FieldTr), item.LocationEn.Or(item.LocationTr), item.StartDate, item.EndDate, item.DescriptionEn.Or(item.DescriptionTr), Lines(item.BulletPointsEn.Or(item.BulletPointsTr)), item.LogoUrl, item.IsExchangeProgram, item.TimelineColor, item.SortOrder);
    private static SkillCategoryDto Map(SkillCategory item) => new(item.Id, item.NameEn.Or(item.NameTr), item.Icon, item.AccentColor, item.SortOrder, item.Skills.Select(skill => new SkillDto(skill.Id, skill.Name, skill.Icon, skill.IconType, skill.IconUrl, skill.BrandColor, skill.DescriptionEn.Or(skill.DescriptionTr), skill.Level, skill.YearsOfExperience, skill.SortOrder)).ToList());
    private static ProjectDto Map(Project item) => new(item.Id, item.TitleEn.Or(item.Title), item.Slug, item.CategoryEn.Or(item.Status), item.ShortDescriptionEn.Or(item.DescriptionEn.PlainSummary()), item.DescriptionEn.Or(item.Description), item.ImageUrl, Csv(item.Screenshots), item.Tags, item.GitHubUrl, item.ProjectUrl, item.CaseStudyUrl, item.IsFeatured, item.SortOrder);
    private static LanguageDto Map(SpokenLanguage item) => new(item.Id, item.NameEn.Or(item.NameTr), item.LevelEn.Or(item.LevelTr), item.Percentage, item.Icon, item.SortOrder);
    private static BlogPostDto Map(Article item) => new(item.Id, item.TitleEn.Or(item.Title), item.Slug, item.SummaryEn.Or(item.Summary), item.ContentEn.Or(item.Content), item.CoverImage, item.CategoryEn.Or("Engineering"), Csv(item.Tags), item.PublishedAt ?? item.CreatedAt, ReadingTime(item.ContentEn.Or(item.Content)), item.SeoTitle.Or(item.TitleEn.Or(item.Title)), item.SeoDescription.Or(item.SummaryEn.Or(item.Summary)), item.IsFeatured, item.SortOrder);
    private static List<string> Lines(string value) => value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    private static List<string> Csv(string value) => value.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    private static int ReadingTime(string html) => Math.Max(1, (int)Math.Ceiling(Regex.Replace(html, "<[^>]+>", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries).Length / 220d));
}
