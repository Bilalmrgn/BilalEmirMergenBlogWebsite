using BilalEmirMergenWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace BilalEmirMergenWebsite.Data;

public static class PortfolioSeeder
{
    public static async Task SeedAsync(AppDbContext database)
    {
        if (!await database.SiteSettings.AnyAsync())
        {
            database.SiteSettings.Add(new SiteSettings
            {
                Id = "main",
                BrandName = "BEM.dev",
                FullName = "Bilal Emir Mergen",
                TitleEn = "Full Stack Software Engineer",
                SubtitleEn = "I design dependable web products and the systems behind them.",
                HeadlineEn = "Thoughtful software, built to last.",
                IntroEn = "I turn complex product requirements into clear, maintainable applications across the .NET and modern web ecosystem.",
                LocationEn = "Istanbul, Türkiye",
                AvailabilityEn = "Available for selected opportunities",
                IsAvailable = true,
                SeoTitleEn = "Bilal Emir Mergen — Full Stack Software Engineer",
                SeoDescriptionEn = "Portfolio of Bilal Emir Mergen, a full stack software engineer building reliable web applications, APIs and developer-focused products.",
                ContactTitleEn = "Let’s build something useful.",
                ContactTextEn = "Have a product, platform or engineering challenge in mind? I’d be glad to hear about it.",
                CanonicalBaseUrl = "https://bilalmergen.com"
            });
        }

        if (!await database.AboutSections.AnyAsync())
        {
            database.AboutSections.Add(new AboutSection
            {
                EyebrowEn = "Welcome",
                TitleEn = "About me",
                ShortTextEn = "I’m Bilal, a full stack software engineer focused on robust architecture and polished user experiences.",
                DescriptionEn = "<p>I enjoy working where product thinking and engineering discipline meet. My work spans accessible interfaces, dependable APIs, data modeling and the operational details that help software stay healthy after launch.</p><p>I value clear communication, small feedback loops and systems that are easy for the next engineer to understand.</p>",
                SortOrder = 1
            });
        }

        if (!await database.Experiences.AnyAsync())
        {
            database.Experiences.Add(new Experience
            {
                CompanyName = "Q2 Technology",
                RoleEn = "Full Stack Developer",
                LocationEn = "Istanbul, Türkiye",
                StartDate = new DateTime(2025, 5, 1),
                IsCurrent = true,
                DescriptionEn = "Building business-critical web applications and backend services in a collaborative product team.",
                BulletPointsEn = "Developed invoice and workflow features using C# and .NET\nImplemented background jobs and resilient integrations\nImproved performance with caching and query optimization",
                Technologies = "C#, ASP.NET Core, Angular, SQL Server, Redis, Hangfire",
                TimelineColor = "#2F9CF4",
                SortOrder = 1
            });
        }

        if (!await database.Educations.AnyAsync())
        {
            database.Educations.AddRange(
                new Education
                {
                    SchoolName = "Karadeniz Technical University",
                    DegreeEn = "Bachelor of Science",
                    FieldEn = "Software Engineering",
                    LocationEn = "Trabzon, Türkiye",
                    StartDate = new DateTime(2020, 9, 1),
                    EndDate = new DateTime(2026, 1, 1),
                    DescriptionEn = "A software engineering foundation spanning algorithms, databases, distributed systems and team-based product development.",
                    TimelineColor = "#9B6CF0",
                    SortOrder = 1
                },
                new Education
                {
                    SchoolName = "Universidad de Zaragoza",
                    DegreeEn = "Erasmus+ Exchange Program",
                    FieldEn = "Software Engineering",
                    LocationEn = "Zaragoza, Spain",
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 7, 1),
                    IsExchangeProgram = true,
                    DescriptionEn = "An international study experience focused on software systems and cross-cultural collaboration.",
                    TimelineColor = "#F17863",
                    SortOrder = 2
                });
        }

        if (!await database.SkillCategories.AnyAsync())
        {
            var frontend = new SkillCategory { NameEn = "Frontend", Icon = "panels-top-left", AccentColor = "#2F9CF4", SortOrder = 1 };
            var backend = new SkillCategory { NameEn = "Backend", Icon = "braces", AccentColor = "#F17863", SortOrder = 2 };
            var databaseCategory = new SkillCategory { NameEn = "Database", Icon = "database", AccentColor = "#18A999", SortOrder = 3 };
            var tools = new SkillCategory { NameEn = "Other Technologies", Icon = "wrench", AccentColor = "#9B6CF0", SortOrder = 4 };
            database.SkillCategories.AddRange(frontend, backend, databaseCategory, tools);
            database.Skills.AddRange(
                NewSkill("Angular", "simple-icons:angular", frontend, 1),
                NewSkill("TypeScript", "simple-icons:typescript", frontend, 2),
                NewSkill("JavaScript", "simple-icons:javascript", frontend, 3),
                NewSkill("HTML5 & CSS3", "code-2", frontend, 4),
                NewSkill("C#", "simple-icons:dotnet", backend, 1),
                NewSkill("ASP.NET Core", "simple-icons:dotnet", backend, 2),
                NewSkill("Entity Framework Core", "database-zap", backend, 3),
                NewSkill("REST APIs", "waypoints", backend, 4),
                NewSkill("SQL Server", "database", databaseCategory, 1),
                NewSkill("PostgreSQL", "simple-icons:postgresql", databaseCategory, 2),
                NewSkill("Redis", "simple-icons:redis", databaseCategory, 3),
                NewSkill("Git", "simple-icons:git", tools, 1),
                NewSkill("Docker", "simple-icons:docker", tools, 2),
                NewSkill("Azure", "cloud", tools, 3),
                NewSkill("Postman", "simple-icons:postman", tools, 4));
        }

        if (!await database.Projects.AnyAsync())
        {
            database.Projects.Add(new Project
            {
                Title = "Modular Portfolio Platform",
                TitleEn = "Modular Portfolio Platform",
                Slug = "modular-portfolio-platform",
                CategoryEn = "Full Stack",
                ShortDescriptionEn = "A database-driven portfolio and publishing system with secure content management.",
                DescriptionEn = "<p>A maintainable portfolio platform designed around structured content, protected editorial workflows and fast public pages.</p>",
                Architecture = "ASP.NET Core MVC · EF Core · SQL Server",
                Tags = new List<string> { "ASP.NET Core", "EF Core", "SQL Server", "JavaScript" },
                IsFeatured = true,
                IsActive = true,
                SortOrder = 1,
                Status = "Published"
            });
        }

        if (!await database.SpokenLanguages.AnyAsync())
        {
            database.SpokenLanguages.AddRange(
                new SpokenLanguage { NameEn = "Turkish", LevelEn = "Native", CefrLevel = "Native", Percentage = 100, Icon = "message-circle", SortOrder = 1 },
                new SpokenLanguage { NameEn = "English", LevelEn = "Professional working proficiency", CefrLevel = "B2+", Percentage = 82, Icon = "message-circle", SortOrder = 2 },
                new SpokenLanguage { NameEn = "Spanish", LevelEn = "Elementary proficiency", CefrLevel = "A2", Percentage = 35, Icon = "message-circle", SortOrder = 3 });
        }

        if (!await database.Articles.AnyAsync())
        {
            database.Articles.Add(new Article
            {
                Title = "Designing Maintainable Feature Boundaries in ASP.NET Core",
                TitleEn = "Designing Maintainable Feature Boundaries in ASP.NET Core",
                Slug = "maintainable-feature-boundaries-aspnet-core",
                CategoryEn = "Architecture",
                Summary = "Practical techniques for keeping web applications modular as product scope grows.",
                SummaryEn = "Practical techniques for keeping web applications modular as product scope grows.",
                Content = "<p>Healthy architecture is less about adding layers and more about making change predictable.</p><h2>Start with cohesive features</h2><p>Keep the request, validation, business rules and persistence decisions for a feature easy to discover. Dependencies should point toward stable abstractions, but every abstraction should earn its place.</p><pre><code>public sealed record CreateProject(string Title, string Slug);</code></pre><h2>Prefer explicit boundaries</h2><p>Clear ownership reduces accidental coupling and makes tests describe real product behavior.</p>",
                ContentEn = "<p>Healthy architecture is less about adding layers and more about making change predictable.</p><h2>Start with cohesive features</h2><p>Keep the request, validation, business rules and persistence decisions for a feature easy to discover. Dependencies should point toward stable abstractions, but every abstraction should earn its place.</p><pre><code>public sealed record CreateProject(string Title, string Slug);</code></pre><h2>Prefer explicit boundaries</h2><p>Clear ownership reduces accidental coupling and makes tests describe real product behavior.</p>",
                Tags = "ASP.NET Core,Architecture,C#",
                ContentLanguage = "en",
                IsActive = true,
                IsFeatured = true,
                PublishedAt = DateTime.UtcNow.AddDays(-14),
                SeoTitle = "Maintainable Feature Boundaries in ASP.NET Core",
                SeoDescription = "A practical guide to keeping ASP.NET Core applications modular and easy to evolve.",
                SortOrder = 1
            });
        }

        if (!await database.Socials.AnyAsync())
        {
            database.Socials.AddRange(
                new Social { Name = "GitHub", Icon = "github", Url = "https://github.com/Bilalmrgn", SortOrder = 1 });
        }

        var legacyToolsCategories = await database.SkillCategories
            .Where(category => category.NameEn == "DevOps & Tools")
            .ToListAsync();
        foreach (var category in legacyToolsCategories) category.NameEn = "Other Technologies";

        var settings = await database.SiteSettings.FirstOrDefaultAsync();
        if (settings is not null && (string.IsNullOrWhiteSpace(settings.OpenGraphImage) || settings.OpenGraphImage is "/favicon.png" or "/og.png")) settings.OpenGraphImage = "/og-portfolio-light.png";

        await database.SaveChangesAsync();
    }

    private static Skill NewSkill(string name, string icon, SkillCategory category, int order) => new()
    {
        Name = name,
        Icon = icon,
        IconType = icon.StartsWith("simple-icons:", StringComparison.Ordinal) ? "simple-icons" : "lucide",
        Category = category,
        CategoryId = category.Id,
        Level = 85,
        SortOrder = order
    };
}
