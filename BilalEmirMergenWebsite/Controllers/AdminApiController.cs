using System.Text.RegularExpressions;
using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Dtos;
using BilalEmirMergenWebsite.Models;
using BilalEmirMergenWebsite.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BilalEmirMergenWebsite.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public sealed class AdminApiController(AppDbContext database, IPasswordService passwords, IJwtTokenService tokens, IImageService images) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("auth/login")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var candidates = await database.AdminUsers.Where(user => (user.Email == request.Email || user.Username == request.Email) && user.Role == "Admin").ToListAsync(cancellationToken);
        var admin = candidates.FirstOrDefault(user => passwords.Verify(request.Password, user.PasswordHash));
        if (admin is null) return Unauthorized(new ProblemDetails { Title = "Invalid credentials", Status = StatusCodes.Status401Unauthorized });
        var result = tokens.Create(admin);
        return new TokenResponse(result.Token, result.ExpiresAtUtc);
    }

    [HttpPost("experiences")]
    public Task<ActionResult<Experience>> CreateExperience(Experience input, CancellationToken cancellationToken) => Create(input, database.Experiences, cancellationToken);
    [HttpPut("experiences/{id}")]
    public Task<ActionResult<Experience>> UpdateExperience(string id, Experience input, CancellationToken cancellationToken) => Update(id, input, database.Experiences, cancellationToken);
    [HttpDelete("experiences/{id}")]
    public Task<IActionResult> DeleteExperience(string id, CancellationToken cancellationToken) => Delete(id, database.Experiences, cancellationToken);

    [HttpPost("educations")]
    public Task<ActionResult<Education>> CreateEducation(Education input, CancellationToken cancellationToken) => Create(input, database.Educations, cancellationToken);
    [HttpPut("educations/{id}")]
    public Task<ActionResult<Education>> UpdateEducation(string id, Education input, CancellationToken cancellationToken) => Update(id, input, database.Educations, cancellationToken);
    [HttpDelete("educations/{id}")]
    public Task<IActionResult> DeleteEducation(string id, CancellationToken cancellationToken) => Delete(id, database.Educations, cancellationToken);

    [HttpPost("skill-categories")]
    public Task<ActionResult<SkillCategory>> CreateSkillCategory(SkillCategory input, CancellationToken cancellationToken) => Create(input, database.SkillCategories, cancellationToken);
    [HttpPut("skill-categories/{id}")]
    public Task<ActionResult<SkillCategory>> UpdateSkillCategory(string id, SkillCategory input, CancellationToken cancellationToken) => Update(id, input, database.SkillCategories, cancellationToken);
    [HttpDelete("skill-categories/{id}")]
    public Task<IActionResult> DeleteSkillCategory(string id, CancellationToken cancellationToken) => Delete(id, database.SkillCategories, cancellationToken);

    [HttpPost("skills")]
    public Task<ActionResult<Skill>> CreateSkill(Skill input, CancellationToken cancellationToken) { input.Category = null; return Create(input, database.Skills, cancellationToken); }
    [HttpPut("skills/{id}")]
    public Task<ActionResult<Skill>> UpdateSkill(string id, Skill input, CancellationToken cancellationToken) { input.Category = null; return Update(id, input, database.Skills, cancellationToken); }
    [HttpDelete("skills/{id}")]
    public Task<IActionResult> DeleteSkill(string id, CancellationToken cancellationToken) => Delete(id, database.Skills, cancellationToken);

    [HttpPost("projects")]
    public Task<ActionResult<Project>> CreateProject(Project input, CancellationToken cancellationToken)
    {
        Prepare(input);
        return Create(input, database.Projects, cancellationToken);
    }
    [HttpPut("projects/{id}")]
    public Task<ActionResult<Project>> UpdateProject(string id, Project input, CancellationToken cancellationToken)
    {
        Prepare(input);
        return Update(id, input, database.Projects, cancellationToken);
    }
    [HttpDelete("projects/{id}")]
    public Task<IActionResult> DeleteProject(string id, CancellationToken cancellationToken) => Delete(id, database.Projects, cancellationToken);

    [HttpPost("languages")]
    public Task<ActionResult<SpokenLanguage>> CreateLanguage(SpokenLanguage input, CancellationToken cancellationToken) => Create(input, database.SpokenLanguages, cancellationToken);
    [HttpPut("languages/{id}")]
    public Task<ActionResult<SpokenLanguage>> UpdateLanguage(string id, SpokenLanguage input, CancellationToken cancellationToken) => Update(id, input, database.SpokenLanguages, cancellationToken);
    [HttpDelete("languages/{id}")]
    public Task<IActionResult> DeleteLanguage(string id, CancellationToken cancellationToken) => Delete(id, database.SpokenLanguages, cancellationToken);

    [HttpPost("blog")]
    public Task<ActionResult<Article>> CreateBlogPost(Article input, CancellationToken cancellationToken)
    {
        Prepare(input);
        return Create(input, database.Articles, cancellationToken);
    }
    [HttpPut("blog/{id}")]
    public Task<ActionResult<Article>> UpdateBlogPost(string id, Article input, CancellationToken cancellationToken)
    {
        Prepare(input);
        return Update(id, input, database.Articles, cancellationToken);
    }
    [HttpDelete("blog/{id}")]
    public Task<IActionResult> DeleteBlogPost(string id, CancellationToken cancellationToken) => Delete(id, database.Articles, cancellationToken);

    [HttpPost("social-links")]
    public Task<ActionResult<Social>> CreateSocialLink(Social input, CancellationToken cancellationToken) => Create(input, database.Socials, cancellationToken);
    [HttpPut("social-links/{id}")]
    public Task<ActionResult<Social>> UpdateSocialLink(string id, Social input, CancellationToken cancellationToken) => Update(id, input, database.Socials, cancellationToken);
    [HttpDelete("social-links/{id}")]
    public Task<IActionResult> DeleteSocialLink(string id, CancellationToken cancellationToken) => Delete(id, database.Socials, cancellationToken);

    [HttpPut("{type}/reorder")]
    public async Task<IActionResult> Reorder(string type, IReadOnlyList<ReorderItem> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0) return BadRequest(new ProblemDetails { Title = "At least one item is required." });
        switch (type.ToLowerInvariant())
        {
            case "experiences": await ApplyOrder(database.Experiences, items, cancellationToken); break;
            case "educations": await ApplyOrder(database.Educations, items, cancellationToken); break;
            case "skill-categories": await ApplyOrder(database.SkillCategories, items, cancellationToken); break;
            case "skills": await ApplyOrder(database.Skills, items, cancellationToken); break;
            case "projects": await ApplyOrder(database.Projects, items, cancellationToken); break;
            case "languages": await ApplyOrder(database.SpokenLanguages, items, cancellationToken); break;
            case "blog": await ApplyOrder(database.Articles, items, cancellationToken); break;
            case "social-links": await ApplyOrder(database.Socials, items, cancellationToken); break;
            default: return NotFound();
        }
        await database.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("uploads/{kind}")]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<ActionResult<object>> Upload(string kind, [FromForm] UploadRequest request, CancellationToken cancellationToken)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "profile", "projects", "project-gallery", "blog", "education", "companies" };
        if (!allowed.Contains(kind)) return BadRequest(new ProblemDetails { Title = "Unsupported upload kind." });
        var path = await images.SaveImageAsync(request.File, kind, cancellationToken);
        return Ok(new { url = path });
    }

    private async Task<ActionResult<T>> Create<T>(T input, DbSet<T> set, CancellationToken cancellationToken) where T : class
    {
        input.NormalizeText();
        typeof(T).GetProperty("Id")?.SetValue(input, Guid.NewGuid().ToString());
        set.Add(input);
        await database.SaveChangesAsync(cancellationToken);
        var id = (string?)typeof(T).GetProperty("Id")?.GetValue(input);
        return Created($"/api/admin/{typeof(T).Name.ToLowerInvariant()}/{id}", input);
    }

    private async Task<ActionResult<T>> Update<T>(string id, T input, DbSet<T> set, CancellationToken cancellationToken) where T : class
    {
        input.NormalizeText();
        var existing = await set.FindAsync(new object[] { id }, cancellationToken);
        if (existing is null) return NotFound();
        typeof(T).GetProperty("Id")?.SetValue(input, id);
        database.Entry(existing).CurrentValues.SetValues(input);
        await database.SaveChangesAsync(cancellationToken);
        return Ok(existing);
    }

    private async Task<IActionResult> Delete<T>(string id, DbSet<T> set, CancellationToken cancellationToken) where T : class
    {
        var existing = await set.FindAsync(new object[] { id }, cancellationToken);
        if (existing is null) return NotFound();
        set.Remove(existing);
        await database.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static async Task ApplyOrder<T>(DbSet<T> set, IReadOnlyList<ReorderItem> items, CancellationToken cancellationToken) where T : class, IOrderedContent
    {
        var order = items.ToDictionary(item => item.Id, item => item.DisplayOrder);
        var ids = order.Keys.ToList();
        var entities = await set.Where(item => ids.Contains(EF.Property<string>(item, "Id"))).ToListAsync(cancellationToken);
        foreach (var entity in entities) entity.SortOrder = order[(string)entity.GetType().GetProperty("Id")!.GetValue(entity)!];
    }

    private static void Prepare(Project project)
    {
        project.Title = project.TitleEn.Or(project.Title);
        project.Description = project.DescriptionEn.Or(project.Description);
        project.Slug = Slug(project.Slug.Or(project.Title));
        project.UpdatedAt = DateTime.UtcNow;
    }

    private static void Prepare(Article article)
    {
        article.Title = article.TitleEn.Or(article.Title);
        article.Summary = article.SummaryEn.Or(article.Summary);
        article.Content = article.ContentEn.Or(article.Content);
        article.Slug = Slug(article.Slug.Or(article.Title));
        article.UpdatedAt = DateTime.UtcNow;
    }

    private static string Slug(string value) => Regex.Replace(value.ToLowerInvariant().Replace('ı', 'i').Replace('ğ', 'g').Replace('ü', 'u').Replace('ş', 's').Replace('ö', 'o').Replace('ç', 'c'), "[^a-z0-9]+", "-").Trim('-');
}
