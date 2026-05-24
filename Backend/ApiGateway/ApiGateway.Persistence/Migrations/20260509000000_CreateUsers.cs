using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiGateway.Persistence.Migrations;

/// <summary>
/// Creates the initial ApiGateway users table and ownership indexes.
/// </summary>
[DbContext(typeof(ApiGatewayDbContext))]
[Migration("20260509000000_CreateUsers")]
public partial class CreateUsers : Migration
{
    /// <summary>
    /// Applies the initial users schema.
    /// </summary>
    /// <param name="migrationBuilder">Builder used to apply schema operations.</param>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                keycloak_subject = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                nickname = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                current_interview_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_users_current_interview_id",
            table: "users",
            column: "current_interview_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_keycloak_subject",
            table: "users",
            column: "keycloak_subject",
            unique: true);
    }

    /// <summary>
    /// Removes the initial users schema.
    /// </summary>
    /// <param name="migrationBuilder">Builder used to apply rollback schema operations.</param>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "users");
    }
}
