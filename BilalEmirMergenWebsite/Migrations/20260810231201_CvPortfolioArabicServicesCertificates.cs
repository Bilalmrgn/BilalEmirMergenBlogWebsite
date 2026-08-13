using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilalEmirMergenWebsite.Migrations
{
    /// <inheritdoc />
    public partial class CvPortfolioArabicServicesCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LevelAr",
                table: "SpokenLanguages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "SpokenLanguages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionTr",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IconType",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "SkillCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "SkillCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvailabilityAr",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactTextAr",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactTitleAr",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeadlineAr",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IntroAr",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LocationAr",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeoDescriptionAr",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeoTitleAr",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubtitleAr",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryAr",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryEn",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryTr",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionAr",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BulletPointsAr",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BulletPointsEn",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BulletPointsTr",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LocationAr",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoleAr",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "EngineeringConcepts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "EngineeringConcepts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DegreeAr",
                table: "Educations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Educations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FieldAr",
                table: "Educations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "Educations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocationAr",
                table: "Educations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryAr",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryEn",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryTr",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentAr",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SummaryAr",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "AcademicFoundations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "AcademicFoundations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "AboutSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EyebrowAr",
                table: "AboutSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EyebrowEn",
                table: "AboutSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EyebrowTr",
                table: "AboutSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortTextAr",
                table: "AboutSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "AboutSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameTr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Organization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoesNotExpire = table.Column<bool>(type: "bit", nullable: false),
                    CredentialId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CredentialUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionTr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CertificateImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizationLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleTr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionTr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccentColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropColumn(
                name: "LevelAr",
                table: "SpokenLanguages");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "SpokenLanguages");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "DescriptionTr",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "IconType",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "SkillCategories");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "SkillCategories");

            migrationBuilder.DropColumn(
                name: "AvailabilityAr",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ContactTextAr",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ContactTitleAr",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "HeadlineAr",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "IntroAr",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "LocationAr",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SeoDescriptionAr",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SeoTitleAr",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SubtitleAr",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "CategoryAr",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CategoryEn",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CategoryTr",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ShortDescriptionAr",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "BulletPointsAr",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "BulletPointsEn",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "BulletPointsTr",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "LocationAr",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "RoleAr",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "EngineeringConcepts");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "EngineeringConcepts");

            migrationBuilder.DropColumn(
                name: "DegreeAr",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "FieldAr",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "LocationAr",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "CategoryAr",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "CategoryEn",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "CategoryTr",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ContentAr",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "SummaryAr",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "AcademicFoundations");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "AcademicFoundations");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "AboutSections");

            migrationBuilder.DropColumn(
                name: "EyebrowAr",
                table: "AboutSections");

            migrationBuilder.DropColumn(
                name: "EyebrowEn",
                table: "AboutSections");

            migrationBuilder.DropColumn(
                name: "EyebrowTr",
                table: "AboutSections");

            migrationBuilder.DropColumn(
                name: "ShortTextAr",
                table: "AboutSections");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "AboutSections");
        }
    }
}
