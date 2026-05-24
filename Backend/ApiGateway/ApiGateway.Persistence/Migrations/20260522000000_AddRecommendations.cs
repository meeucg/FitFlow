using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiGateway.Persistence.Migrations;

/// <summary>
/// Adds ApiGateway-owned recommendation initialization and recommendation state tables.
/// </summary>
[DbContext(typeof(ApiGatewayDbContext))]
[Migration("20260522000000_AddRecommendations")]
public partial class AddRecommendations : Migration
{
    /// <summary>
    /// Applies recommendation state schema changes.
    /// </summary>
    /// <param name="migrationBuilder">Builder used to apply schema operations.</param>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "recommendation_state",
            table: "users",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "not_started");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "recommendation_requested_at",
            table: "users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "recommendation_initialized_at",
            table: "users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "recommendation_retry_count",
            table: "users",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "recommendation_last_error",
            table: "users",
            type: "text",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "job_recommendations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                job_posting_id = table.Column<Guid>(type: "uuid", nullable: false),
                recommended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_job_recommendations", x => x.id);
                table.ForeignKey(
                    name: "fk_job_recommendations_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_job_recommendations_job_posting_id",
            table: "job_recommendations",
            column: "job_posting_id");

        migrationBuilder.CreateIndex(
            name: "ix_job_recommendations_user_id_job_posting_id",
            table: "job_recommendations",
            columns: ["user_id", "job_posting_id"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_job_recommendations_user_id_recommended_at",
            table: "job_recommendations",
            columns: ["user_id", "recommended_at"]);
    }

    /// <summary>
    /// Removes recommendation state schema changes.
    /// </summary>
    /// <param name="migrationBuilder">Builder used to apply rollback schema operations.</param>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "job_recommendations");

        migrationBuilder.DropColumn(name: "recommendation_state", table: "users");
        migrationBuilder.DropColumn(name: "recommendation_requested_at", table: "users");
        migrationBuilder.DropColumn(name: "recommendation_initialized_at", table: "users");
        migrationBuilder.DropColumn(name: "recommendation_retry_count", table: "users");
        migrationBuilder.DropColumn(name: "recommendation_last_error", table: "users");
    }
}
