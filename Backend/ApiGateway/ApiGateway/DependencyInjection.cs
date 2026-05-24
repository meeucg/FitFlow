using ApiGateway.Application;
using ApiGateway.Application.Abstractions;
using ApiGateway.Authentication;
using ApiGateway.Infrastructure;
using ApiGateway.Options;
using ApiGateway.Persistence;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ApiGateway;

internal static class DependencyInjection
{
    public static IServiceCollection AddApiGateway(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddApiGatewayOptions(configuration)
            .AddApiGatewayCors(configuration)
            .AddApplication()
            .AddPersistence(configuration)
            .AddInfrastructure(configuration)
            .AddApiGatewayHost()
            .AddApiGatewayAuthentication(environment);

        services.AddAuthorization();
        return services;
    }

    private static IServiceCollection AddApiGatewayOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatedOptions<AuthenticationOptions, AuthenticationOptionsValidator>(
            configuration,
            AuthenticationOptions.SectionName);
        services.AddValidatedOptions<RabbitMqOptions, RabbitMqOptionsValidator>(
            configuration,
            RabbitMqOptions.SectionName);
        services.AddValidatedOptions<RecommendationsOptions, RecommendationsOptionsValidator>(
            configuration,
            RecommendationsOptions.SectionName);

        return services;
    }

    private static IServiceCollection AddApiGatewayCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    private static IServiceCollection AddApiGatewayHost(this IServiceCollection services)
    {
        services.AddSingleton<RecommendationSseHub>();
        services.AddSingleton<IRecommendationNotifier>(provider => provider.GetRequiredService<RecommendationSseHub>());
        services.AddHostedService<RecommendationInitializerHostedService>();
        services.AddHostedService<FeedCoreRecommendationConsumerHostedService>();
        return services;
    }

    private static IServiceCollection AddApiGatewayAuthentication(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AuthenticationOptions>>(
                (options, authOptions) =>
                {
                    var settings = authOptions.Value;
                    options.Authority = settings.Authority!;
                    options.MetadataAddress = settings.MetadataAddress!;
                    options.Audience = settings.Audience!;
                    options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
                    options.IncludeErrorDetails = environment.IsDevelopment();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = settings.Authority!,
                        ValidateAudience = true,
                        ValidAudience = settings.Audience!,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                    };
                    options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        settings.MetadataAddress!,
                        new OpenIdConnectConfigurationRetriever(),
                        new AuthorityRewritingDocumentRetriever(
                            settings.Authority!,
                            settings.BackchannelAuthority,
                            settings.RequireHttpsMetadata));
                });

        return services;
    }

    private static IServiceCollection AddValidatedOptions<TOptions, TValidator>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
        where TValidator : class, IValidateOptions<TOptions>
    {
        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<TOptions>, TValidator>();
        return services;
    }
}
