using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilalEmirMergenWebsite.Migrations;

/// <summary>
/// Repairs the legacy dbo.AdminUsers table without replacing the existing
/// administrator or password hash.
/// </summary>
public partial class RepairDboAdminUserColumns20260814 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'[dbo].[AdminUsers]', N'Username') IS NULL
            BEGIN
                ALTER TABLE [dbo].[AdminUsers]
                    ADD [Username] nvarchar(max) NOT NULL
                    CONSTRAINT [DF_AdminUsers_Username_20260814] DEFAULT N'';
            END

            IF COL_LENGTH(N'[dbo].[AdminUsers]', N'Role') IS NULL
            BEGIN
                ALTER TABLE [dbo].[AdminUsers]
                    ADD [Role] nvarchar(max) NOT NULL
                    CONSTRAINT [DF_AdminUsers_Role_20260814] DEFAULT N'Admin';
            END

            EXEC(N'
                UPDATE [dbo].[AdminUsers]
                SET [Username] = CASE
                        WHEN NULLIF(LTRIM(RTRIM([Username])), N'''') IS NOT NULL THEN [Username]
                        WHEN CHARINDEX(N''@'', [Email]) > 1 THEN LEFT([Email], CHARINDEX(N''@'', [Email]) - 1)
                        ELSE N''admin''
                    END,
                    [Role] = CASE
                        WHEN NULLIF(LTRIM(RTRIM([Role])), N'''') IS NOT NULL THEN [Role]
                        ELSE N''Admin''
                    END;
            ');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Existing authentication data must not be modified automatically.
    }
}
