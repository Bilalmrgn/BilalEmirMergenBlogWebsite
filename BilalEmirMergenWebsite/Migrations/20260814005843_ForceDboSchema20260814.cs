using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilalEmirMergenWebsite.Migrations;

/// <summary>
/// Pins every EF Core entity to dbo and repairs databases where migrations
/// created the newer content tables in the SQL login's default schema.
/// Existing dbo tables are deliberately preserved.
/// </summary>
public partial class ForceDboSchema20260814 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema("dbo");

        foreach (var table in new[]
        {
            "AboutSections",
            "AcademicFoundations",
            "AdminUsers",
            "Analytics",
            "Articles",
            "Certificates",
            "Educations",
            "EngineeringConcepts",
            "Experiences",
            "Projects",
            "Services",
            "SiteSettings",
            "SkillCategories",
            "Skills",
            "Socials",
            "SpokenLanguages"
        })
        {
            migrationBuilder.Sql($$"""
                IF OBJECT_ID(N'[dbo].[{{table}}]', N'U') IS NULL
                   AND OBJECT_ID(N'[bilalmrgn].[{{table}}]', N'U') IS NOT NULL
                BEGIN
                    ALTER SCHEMA [dbo] TRANSFER [bilalmrgn].[{{table}}];
                END
                """);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally left empty. Reintroducing login-dependent schemas would
        // split production data again and is not a safe automatic rollback.
    }
}
