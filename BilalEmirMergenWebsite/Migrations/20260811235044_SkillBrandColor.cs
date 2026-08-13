using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilalEmirMergenWebsite.Migrations
{
    /// <inheritdoc />
    public partial class SkillBrandColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrandColor",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "#2F9CF4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandColor",
                table: "Skills");
        }
    }
}
