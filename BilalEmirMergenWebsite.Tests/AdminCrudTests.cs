using System.Net;
using System.Text.RegularExpressions;
using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Models;
using BilalEmirMergenWebsite.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BilalEmirMergenWebsite.Tests;

public sealed class AdminCrudTests : IClassFixture<PortfolioWebApplicationFactory>
{
    private readonly PortfolioWebApplicationFactory _factory;

    public AdminCrudTests(PortfolioWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Targeted_admin_crud_flows_accept_empty_optional_fields_and_persist_changes()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await _factory.SeedAsync();
        await using (var authenticationScope = _factory.Services.CreateAsyncScope())
        {
            var authenticationDatabase = authenticationScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var authenticationPasswords = authenticationScope.ServiceProvider.GetRequiredService<IPasswordService>();
            var admin = await authenticationDatabase.AdminUsers.SingleAsync();
            Assert.Equal(PortfolioWebApplicationFactory.AdminEmail, admin.Email);
            Assert.True(authenticationPasswords.Verify(PortfolioWebApplicationFactory.AdminPassword, admin.PasswordHash));
        }
        await SignIn(client);

        await using var initialScope = _factory.Services.CreateAsyncScope();
        var initialDatabase = initialScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = await initialDatabase.SkillCategories.AsNoTracking().SingleAsync();
        var skill = await initialDatabase.Skills.AsNoTracking().SingleAsync();
        var education = await initialDatabase.Educations.AsNoTracking().SingleAsync();
        var about = await initialDatabase.AboutSections.AsNoTracking().SingleAsync();

        var skillsPage = await GetPage(client, "/admin/dashboard?tab=skills&editor=technology&editId=" + skill.Id);
        Assert.Matches($"<option[^>]*(value=\"{Regex.Escape(category.Id)}\"[^>]*selected|selected[^>]*value=\"{Regex.Escape(category.Id)}\")", skillsPage);
        var skillsToken = AntiforgeryToken(skillsPage);
        var skillResponse = await client.PostAsync("/admin/skill/save", Form(
            ("__RequestVerificationToken", skillsToken), ("Id", skill.Id), ("Name", "PostgreSQL 16"),
            ("CategoryId", category.Id), ("Icon", "postgresql"), ("IconType", "simple-icons"),
            ("BrandColor", "#4169E1"), ("IsActive", "false"), ("IsActive", "true"), ("SortOrder", "2")));
        AssertRedirect(skillResponse, "skills");

        var educationPage = await GetPage(client, "/admin/dashboard?tab=education&editId=" + education.Id);
        var educationResponse = await client.PostAsync("/admin/education/save", Form(
            ("__RequestVerificationToken", AntiforgeryToken(educationPage)), ("Id", education.Id),
            ("SchoolName", "Portfolio University"), ("DegreeEn", "BSc"), ("FieldEn", "Software Engineering"),
            ("TimelineColor", "#9B6CF0"), ("IsActive", "true"), ("IsActive", "false"), ("SortOrder", "1")));
        AssertRedirect(educationResponse, "education");

        var aboutPage = await GetPage(client, "/admin/dashboard?tab=about&editId=" + about.Id);
        Assert.Contains("data-rte-value-command=\"fontName\"", aboutPage, StringComparison.Ordinal);
        const string aboutHtml = "<h2 style=\"font-family: Bodoni Moda\">Editorial profile</h2><p><strong>Polished</strong> engineering.</p>";
        var aboutResponse = await client.PostAsync("/admin/about/save", Form(
            ("__RequestVerificationToken", AntiforgeryToken(aboutPage)), ("Id", about.Id),
            ("TitleEn", "About me"), ("DescriptionEn", aboutHtml), ("IsActive", "true"), ("SortOrder", "1")));
        AssertRedirect(aboutResponse, "about");

        var socialsPage = await GetPage(client, "/admin/dashboard?tab=socials");
        Assert.Contains("data-social-icon-option", socialsPage, StringComparison.Ordinal);
        var socialResponse = await client.PostAsync("/admin/social/save", Form(
            ("__RequestVerificationToken", AntiforgeryToken(socialsPage)), ("Name", "LinkedIn"),
            ("Icon", "linkedin"), ("Url", "https://www.linkedin.com/in/test-profile"),
            ("IsActive", "false"), ("IsActive", "true"), ("SortOrder", "2")));
        AssertRedirect(socialResponse, "socials");

        await using var verifyScope = _factory.Services.CreateAsyncScope();
        var database = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedSkill = (await database.Skills.FindAsync(skill.Id))!;
        Assert.Equal("PostgreSQL 16", savedSkill.Name);
        Assert.True(savedSkill.IsActive);
        var savedEducation = (await database.Educations.FindAsync(education.Id))!;
        Assert.Equal("Portfolio University", savedEducation.SchoolName);
        Assert.True(savedEducation.IsActive);
        Assert.Equal(aboutHtml, (await database.AboutSections.FindAsync(about.Id))!.DescriptionEn);
        var social = await database.Socials.SingleAsync(item => item.Name == "LinkedIn");
        Assert.True(social.IsActive);

        var editSocialPage = await GetPage(client, "/admin/dashboard?tab=socials&editId=" + social.Id);
        var updateSocialResponse = await client.PostAsync("/admin/social/save", Form(
            ("__RequestVerificationToken", AntiforgeryToken(editSocialPage)), ("Id", social.Id),
            ("Name", "Professional profile"), ("Icon", "briefcase-business"),
            ("Url", "https://example.com/profile"), ("IsActive", "true"), ("SortOrder", "2")));
        AssertRedirect(updateSocialResponse, "socials");
        database.ChangeTracker.Clear();
        Assert.Equal("Professional profile", (await database.Socials.FindAsync(social.Id))!.Name);

        var deletePage = await GetPage(client, "/admin/dashboard?tab=socials");
        var deleteResponse = await client.PostAsync("/admin/delete/social/" + social.Id, Form(
            ("__RequestVerificationToken", AntiforgeryToken(deletePage))));
        AssertRedirect(deleteResponse, "socials");
        database.ChangeTracker.Clear();
        Assert.Null(await database.Socials.FindAsync(social.Id));
    }

    private static async Task SignIn(HttpClient client)
    {
        var loginPage = await GetPage(client, "/admin/login");
        var response = await client.PostAsync("/admin/login", Form(
            ("__RequestVerificationToken", AntiforgeryToken(loginPage)),
            ("email", PortfolioWebApplicationFactory.AdminEmail),
            ("password", PortfolioWebApplicationFactory.AdminPassword)));
        Assert.True(response.StatusCode == HttpStatusCode.Redirect, $"Login returned {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        Assert.Equal("/admin/dashboard", response.Headers.Location?.OriginalString);
    }

    private static async Task<string> GetPage(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static string AntiforgeryToken(string html)
    {
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success, "The page did not include an antiforgery token.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static FormUrlEncodedContent Form(params (string Key, string Value)[] values) =>
        new(values.Select(value => new KeyValuePair<string, string>(value.Key, value.Value)));

    private static void AssertRedirect(HttpResponseMessage response, string tab)
    {
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains($"tab={tab}", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }
}

public sealed class PortfolioWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@portfolio.test";
    public const string AdminPassword = "Testing2026";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Key", "portfolio-tests-only-jwt-key-2026-at-least-32-bytes");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            var databaseName = "portfolio-admin-crud-" + Guid.NewGuid();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
        });
    }

    public async Task SeedAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (await database.AdminUsers.AnyAsync()) return;
        var passwords = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        var category = new SkillCategory { Id = "database-category", NameEn = "Database", SortOrder = 1 };
        database.AdminUsers.Add(new AdminUser { Id = "test-admin", Email = AdminEmail, Username = "admin", PasswordHash = passwords.Hash(AdminPassword), Role = "Admin" });
        database.SkillCategories.Add(category);
        database.Skills.Add(new Skill { Id = "postgresql", Name = "PostgreSQL", CategoryId = category.Id, Icon = "postgresql", BrandColor = "#4169E1", SortOrder = 2 });
        database.Educations.Add(new Education { Id = "education", SchoolName = "University", SortOrder = 1 });
        database.AboutSections.Add(new AboutSection { Id = "about", TitleEn = "About", DescriptionEn = "<p>About.</p>", SortOrder = 1 });
        await database.SaveChangesAsync();
    }
}
