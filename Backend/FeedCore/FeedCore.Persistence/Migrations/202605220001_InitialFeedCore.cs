using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedCore.Persistence.Migrations;

[DbContext(typeof(FeedCoreDbContext))]
[Migration("202605220001_InitialFeedCore")]
public partial class InitialFeedCore : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

        migrationBuilder.CreateTable(
            name: "job_postings",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                posted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                received_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                display_json = table.Column<string>(type: "jsonb", nullable: false),
                embedding = table.Column<string>(type: "vector(1536)", nullable: true),
                embedding_state = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                embedding_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                embedded_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                next_embedding_attempt_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                last_embedding_error = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_job_postings", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "recommendation_outbox_messages",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                exchange = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                routing_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                body_json = table.Column<string>(type: "jsonb", nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                next_attempt_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                published_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                last_error = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recommendation_outbox_messages", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "user_profile_embeddings",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                embedding = table.Column<string>(type: "vector(1536)", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_profile_embeddings", x => x.user_id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_job_postings_embedding_state_next_embedding_attempt_at",
            table: "job_postings",
            columns: ["embedding_state", "next_embedding_attempt_at"]);

        migrationBuilder.CreateIndex(
            name: "ix_job_postings_posted_at",
            table: "job_postings",
            column: "posted_at");

        migrationBuilder.CreateIndex(
            name: "ix_job_postings_source_url",
            table: "job_postings",
            columns: ["source", "url"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recommendation_outbox_messages_status_next_attempt_at",
            table: "recommendation_outbox_messages",
            columns: ["status", "next_attempt_at"]);

        migrationBuilder.Sql(
            "CREATE INDEX ix_job_postings_embedding_hnsw ON job_postings USING hnsw (embedding vector_cosine_ops) WHERE embedding IS NOT NULL;");

        migrationBuilder.Sql(
            "CREATE INDEX ix_user_profile_embeddings_embedding_hnsw ON user_profile_embeddings USING hnsw (embedding vector_cosine_ops);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "job_postings");
        migrationBuilder.DropTable(name: "recommendation_outbox_messages");
        migrationBuilder.DropTable(name: "user_profile_embeddings");
    }
}
