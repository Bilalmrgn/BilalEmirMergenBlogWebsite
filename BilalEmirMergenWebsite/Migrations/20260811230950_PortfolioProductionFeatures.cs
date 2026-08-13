using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilalEmirMergenWebsite.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioProductionFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "SpokenLanguages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "languages");

            migrationBuilder.AddColumn<int>(
                name: "Percentage",
                table: "SpokenLanguages",
                type: "int",
                nullable: false,
                defaultValue: 80);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Skills",
                type: "int",
                nullable: false,
                defaultValue: 80);

            migrationBuilder.AddColumn<decimal>(
                name: "YearsOfExperience",
                table: "Skills",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccentColor",
                table: "SkillCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "#2F9CF4");

            migrationBuilder.AddColumn<string>(
                name: "CaseStudyUrl",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Projects",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "CompanyLogoUrl",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimelineColor",
                table: "Experiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "#2F9CF4");

            migrationBuilder.AddColumn<string>(
                name: "BulletPointsAr",
                table: "Educations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BulletPointsEn",
                table: "Educations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BulletPointsTr",
                table: "Educations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsExchangeProgram",
                table: "Educations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TimelineColor",
                table: "Educations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "#9B6CF0");

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Articles",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "SpokenLanguages");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "SpokenLanguages");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "AccentColor",
                table: "SkillCategories");

            migrationBuilder.DropColumn(
                name: "CaseStudyUrl",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CompanyLogoUrl",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "TimelineColor",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "BulletPointsAr",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "BulletPointsEn",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "BulletPointsTr",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "IsExchangeProgram",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "TimelineColor",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Articles");
        }
    }
}
