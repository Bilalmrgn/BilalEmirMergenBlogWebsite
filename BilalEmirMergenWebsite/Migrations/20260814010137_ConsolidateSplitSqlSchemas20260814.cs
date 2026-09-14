using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilalEmirMergenWebsite.Migrations;

/// <summary>
/// Repairs the three legacy tables that existed in dbo while newer, complete
/// copies were accidentally created in the SQL login's default schema.
/// </summary>
public partial class ConsolidateSplitSqlSchemas20260814 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ReplaceLegacyTable(migrationBuilder, "Projects", "TitleEn");
        ReplaceLegacyTable(migrationBuilder, "Articles", "TitleEn");
        ReplaceLegacyTable(migrationBuilder, "Socials", "IsActive");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The legacy copies are retained as *_PreSchemaFix_20260814 tables.
        // An automatic rollback would reintroduce the split-schema defect.
    }

    private static void ReplaceLegacyTable(MigrationBuilder migrationBuilder, string table, string requiredColumn)
    {
        migrationBuilder.Sql($$"""
            IF OBJECT_ID(N'[dbo].[{{table}}]', N'U') IS NOT NULL
               AND COL_LENGTH(N'[dbo].[{{table}}]', N'{{requiredColumn}}') IS NULL
               AND OBJECT_ID(N'[bilalmrgn].[{{table}}]', N'U') IS NOT NULL
            BEGIN
                IF OBJECT_ID(N'[dbo].[{{table}}_PreSchemaFix_20260814]', N'U') IS NULL
                BEGIN
                    SELECT * INTO [dbo].[{{table}}_PreSchemaFix_20260814]
                    FROM [dbo].[{{table}}];
                END

                DROP TABLE [dbo].[{{table}}];
                ALTER SCHEMA [dbo] TRANSFER [bilalmrgn].[{{table}}];
            END
            """);
    }
}
