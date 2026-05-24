using ApiGateway.Core;
using ApiGateway.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiGateway.Persistence;

/// <summary>
/// Entity Framework database context for ApiGateway-owned persistence.
/// </summary>
/// <param name="options">Entity Framework options for the ApiGateway database connection.</param>
public sealed class ApiGatewayDbContext(DbContextOptions<ApiGatewayDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Local users synchronized from Keycloak access-token claims.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Recommendation state rows owned by ApiGateway.
    /// </summary>
    public DbSet<JobRecommendation> JobRecommendations => Set<JobRecommendation>();

    /// <summary>
    /// Configures the ApiGateway relational model and indexes.
    /// </summary>
    /// <param name="modelBuilder">Entity Framework model builder used to configure entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.ToTable("users");
        user.HasKey(x => x.Id);
        user.Property(x => x.Id).HasColumnName("id");
        user.Property(x => x.KeycloakSubject).HasColumnName("keycloak_subject").HasMaxLength(128).IsRequired();
        user.Property(x => x.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        user.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(255);
        user.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(255);
        user.Property(x => x.CurrentInterviewId).HasColumnName("current_interview_id");
        user.Property(x => x.RecommendationState)
            .HasColumnName("recommendation_state")
            .HasMaxLength(32)
            .HasConversion(
                state => RecommendationInitializationStateConverter.ToDatabase(state),
                value => RecommendationInitializationStateConverter.FromDatabase(value))
            .IsRequired();
        user.Property(x => x.RecommendationRequestedAt).HasColumnName("recommendation_requested_at");
        user.Property(x => x.RecommendationInitializedAt).HasColumnName("recommendation_initialized_at");
        user.Property(x => x.RecommendationRetryCount).HasColumnName("recommendation_retry_count");
        user.Property(x => x.RecommendationLastError).HasColumnName("recommendation_last_error");
        user.Property(x => x.CreatedAt).HasColumnName("created_at");
        user.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        user.HasIndex(x => x.KeycloakSubject).IsUnique();
        user.HasIndex(x => x.CurrentInterviewId).IsUnique();

        var recommendation = modelBuilder.Entity<JobRecommendation>();
        recommendation.ToTable("job_recommendations");
        recommendation.HasKey(x => x.Id);
        recommendation.Property(x => x.Id).HasColumnName("id");
        recommendation.Property(x => x.UserId).HasColumnName("user_id");
        recommendation.Property(x => x.JobPostingId).HasColumnName("job_posting_id");
        recommendation.Property(x => x.RecommendedAt).HasColumnName("recommended_at");
        recommendation.Property(x => x.Source)
            .HasColumnName("source")
            .HasMaxLength(32)
            .HasConversion(
                source => RecommendationSourceConverter.ToDatabase(source),
                value => RecommendationSourceConverter.FromDatabase(value))
            .IsRequired();
        recommendation.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        recommendation.HasIndex(x => new { x.UserId, x.JobPostingId }).IsUnique();
        recommendation.HasIndex(x => new { x.UserId, x.RecommendedAt });
        recommendation.HasIndex(x => x.JobPostingId);
    }
}
