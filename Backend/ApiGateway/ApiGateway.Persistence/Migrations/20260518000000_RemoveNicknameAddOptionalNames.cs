using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiGateway.Persistence.Migrations;

/// <summary>
/// Removes the legacy nickname column and adds optional first and last name columns.
/// </summary>
[DbContext(typeof(ApiGatewayDbContext))]
[Migration("20260518000000_RemoveNicknameAddOptionalNames")]
public partial class RemoveNicknameAddOptionalNames : Migration
{
    /// <summary>
    /// Applies the user profile schema update.
    /// </summary>
    /// <param name="migrationBuilder">Builder used to apply schema operations.</param>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "nickname",
            table: "users");

        migrationBuilder.AddColumn<string>(
            name: "first_name",
            table: "users",
            type: "character varying(255)",
            maxLength: 255,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "last_name",
            table: "users",
            type: "character varying(255)",
            maxLength: 255,
            nullable: true);
    }

    /// <summary>
    /// Restores the legacy nickname schema.
    /// </summary>
    /// <param name="migrationBuilder">Builder used to apply rollback schema operations.</param>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "first_name",
            table: "users");

        migrationBuilder.DropColumn(
            name: "last_name",
            table: "users");

        migrationBuilder.AddColumn<string>(
            name: "nickname",
            table: "users",
            type: "character varying(128)",
            maxLength: 128,
            nullable: false,
            defaultValue: string.Empty);
    }
}
