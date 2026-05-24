using System.ComponentModel;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AIServices.Abstractions;
using AIServices.Models;
using AIServices.Models.Options;
using AIServices.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIServices.ServiceBuilders;

public static class AIServicesServiceBuilder
{
    public static IServiceCollection AddAIServices(
        this IServiceCollection services,
        IConfigurationSection textAIOptionsSection,
        IConfigurationSection textAIModelsOptionsSection,
        IConfigurationSection? embeddingAIOptionsSection = null,
        IConfigurationSection? embeddingAIModelsOptionsSection = null,
        Action<AIJsonOptions>? configureAIJsonOptions = null)
    {
        services.AddSingleton<IValidatorForAIManager, ValidatorForAIManager>();
        services.AddSingleton<IJsonSchemaHelper, JsonSchemaHelper>();

        var aiJsonOptionsBuilder = services.AddOptions<AIJsonOptions>();

        aiJsonOptionsBuilder.Configure(configureAIJsonOptions ?? ConfigureDefaultAIJsonOptions);

        var hasTextAI = textAIModelsOptionsSection.Exists();
        var hasEmbeddingOptions = embeddingAIOptionsSection?.Exists() == true;
        var hasEmbeddingModels = embeddingAIModelsOptionsSection?.Exists() == true;

        if (!hasTextAI && !hasEmbeddingModels)
            throw new InvalidOperationException(
                "At least one AI model configuration section must be provided.");

        if (hasTextAI)
        {
            services.AddOptions<TextAIOptions>()
                .Bind(textAIOptionsSection);

            services.AddSingleton<IOptions<TextAIModelsOptions>>(_ =>
                Options.Create(BindTextAIModelsOptions(textAIModelsOptionsSection)));

            RegisterTextAIs(services, textAIModelsOptionsSection);
        }


        if (hasEmbeddingOptions != hasEmbeddingModels)
            throw new InvalidOperationException(
                "Embedding AI options and embedding AI models configuration sections must be provided together.");

        if (hasEmbeddingOptions && hasEmbeddingModels)
        {
            services.AddOptions<EmbeddingAIOptions>()
                .Bind(embeddingAIOptionsSection!);

            RegisterEmbeddingAIs(services, embeddingAIModelsOptionsSection!);
        }

        return services;
    }

    private static void RegisterTextAIs(
        IServiceCollection services,
        IConfigurationSection textAIModelsOptionsSection)
    {
        services.AddSingleton<ITextAI>(sp => new TextAI(
            sp.GetRequiredService<IValidatorForAIManager>(),
            sp.GetRequiredService<IJsonSchemaHelper>(),
            sp.GetRequiredService<IOptions<AIJsonOptions>>(),
            sp.GetRequiredService<IOptions<TextAIOptions>>(),
            sp.GetRequiredService<IOptions<TextAIModelsOptions>>().Value.DefaultModel,
            sp.GetService<ILogger<TextAI>>()));

        var textAIModelsOptions = BindTextAIModelsOptions(textAIModelsOptionsSection);

        var allModels = new List<TextAIModel> { textAIModelsOptions.DefaultModel };
        
        allModels.AddRange(textAIModelsOptions.AlternativeModels);

        foreach (var model in allModels
                     .GroupBy(x => x.ModelAlias, StringComparer.Ordinal)
                     .Select(g => g.First()))
        {
            services.AddKeyedSingleton<ITextAI>(model.ModelAlias, (sp, _) => new TextAI(
                sp.GetRequiredService<IValidatorForAIManager>(),
                sp.GetRequiredService<IJsonSchemaHelper>(),
                sp.GetRequiredService<IOptions<AIJsonOptions>>(),
                sp.GetRequiredService<IOptions<TextAIOptions>>(),
                model,
                sp.GetService<ILogger<TextAI>>()));
        }
    }

    private static void RegisterEmbeddingAIs(
        IServiceCollection services,
        IConfigurationSection embeddingAIModelsOptionsSection)
    {
        services.AddSingleton<IOptions<EmbeddingAIModelsOptions>>(_ =>
            Options.Create(BindEmbeddingAIModelsOptions(embeddingAIModelsOptionsSection)));

        services.AddSingleton<IEmbeddingAI>(sp => new EmbeddingAI(
            sp.GetRequiredService<IOptions<EmbeddingAIOptions>>(),
            sp.GetRequiredService<IOptions<EmbeddingAIModelsOptions>>().Value.DefaultModel,
            sp.GetService<ILogger<EmbeddingAI>>()));

        var embeddingAIModelsOptions = BindEmbeddingAIModelsOptions(embeddingAIModelsOptionsSection);

        var allModels = new List<EmbeddingAIModel> { embeddingAIModelsOptions.DefaultModel };

        allModels.AddRange(embeddingAIModelsOptions.AlternativeModels);

        foreach (var model in allModels
                     .GroupBy(x => x.ModelAlias, StringComparer.Ordinal)
                     .Select(g => g.First()))
        {
            services.AddKeyedSingleton<IEmbeddingAI>(model.ModelAlias, (sp, _) => new EmbeddingAI(
                sp.GetRequiredService<IOptions<EmbeddingAIOptions>>(),
                model,
                sp.GetService<ILogger<EmbeddingAI>>()));
        }
    }

    private static TextAIModelsOptions BindTextAIModelsOptions(IConfigurationSection textAIModelsOptionsSection)
    {
        var defaultModelSection = textAIModelsOptionsSection.GetSection(nameof(TextAIModelsOptions.DefaultModel));

        if (!defaultModelSection.Exists())
            throw new InvalidOperationException("Default text AI model configuration section is missing.");

        return new TextAIModelsOptions
        {
            DefaultModel = BindTextAIModel(defaultModelSection),
            AlternativeModels = textAIModelsOptionsSection
                .GetSection(nameof(TextAIModelsOptions.AlternativeModels))
                .GetChildren()
                .Select(BindTextAIModel)
                .ToList()
        };
    }

    private static TextAIModel BindTextAIModel(IConfigurationSection aiModelSection)
    {
        return new TextAIModel
        {
            ModelAlias = aiModelSection[nameof(TextAIModel.ModelAlias)]
                         ?? throw new InvalidOperationException("Text AI model alias is missing."),
            ModelName = aiModelSection[nameof(TextAIModel.ModelName)]
                        ?? throw new InvalidOperationException("Text AI model name is missing."),
            SupportsJsonOutput = aiModelSection.GetValue<bool>(nameof(TextAIModel.SupportsJsonOutput)),
            SupportsFunctionCalling = aiModelSection.GetValue<bool>(nameof(TextAIModel.SupportsFunctionCalling)),
            RequestBodyExtensions = BindJsonObject(
                aiModelSection.GetSection(nameof(TextAIModel.RequestBodyExtensions)))
        };
    }

    private static EmbeddingAIModelsOptions BindEmbeddingAIModelsOptions(
        IConfigurationSection embeddingAIModelsOptionsSection)
    {
        var defaultModelSection =
            embeddingAIModelsOptionsSection.GetSection(nameof(EmbeddingAIModelsOptions.DefaultModel));

        if (!defaultModelSection.Exists())
            throw new InvalidOperationException("Default embedding AI model configuration section is missing.");

        return new EmbeddingAIModelsOptions
        {
            DefaultModel = BindEmbeddingAIModel(defaultModelSection),
            AlternativeModels = embeddingAIModelsOptionsSection
                .GetSection(nameof(EmbeddingAIModelsOptions.AlternativeModels))
                .GetChildren()
                .Select(BindEmbeddingAIModel)
                .ToList()
        };
    }

    private static EmbeddingAIModel BindEmbeddingAIModel(IConfigurationSection embeddingAIModelSection)
    {
        return new EmbeddingAIModel
        {
            ModelAlias = embeddingAIModelSection[nameof(EmbeddingAIModel.ModelAlias)]
                         ?? throw new InvalidOperationException("Embedding AI model alias is missing."),
            ModelName = embeddingAIModelSection[nameof(EmbeddingAIModel.ModelName)]
                        ?? throw new InvalidOperationException("Embedding AI model name is missing."),
            EndUserId = embeddingAIModelSection[nameof(EmbeddingAIModel.EndUserId)],
            SupportsDimensionControl =
                embeddingAIModelSection.GetValue<bool>(nameof(EmbeddingAIModel.SupportsDimensionControl)),
            DimensionCount = embeddingAIModelSection.GetValue<int?>(nameof(EmbeddingAIModel.DimensionCount))
        };
    }

    private static JsonObject BindJsonObject(IConfigurationSection section)
    {
        var result = new JsonObject();

        foreach (var child in section.GetChildren())
            result[child.Key] = BindJsonNode(child);

        return result;
    }

    private static JsonNode? BindJsonNode(IConfigurationSection section)
    {
        var children = section.GetChildren().ToList();

        if (children.Count == 0)
            return BindJsonValue(section.Value);

        if (children.All(child => int.TryParse(child.Key, out _)))
        {
            var array = new JsonArray();

            foreach (var child in children.OrderBy(child => int.Parse(child.Key)))
                array.Add(BindJsonNode(child));

            return array;
        }

        var obj = new JsonObject();

        foreach (var child in children)
            obj[child.Key] = BindJsonNode(child);

        return obj;
    }

    private static JsonNode? BindJsonValue(string? value)
    {
        if (value is null)
            return null;

        if (bool.TryParse(value, out var boolValue))
            return JsonValue.Create(boolValue);

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
            return JsonValue.Create(longValue);

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
            return JsonValue.Create(doubleValue);

        try
        {
            return JsonNode.Parse(value);
        }
        catch (JsonException)
        {
            return JsonValue.Create(value);
        }
    }

    private static void ConfigureDefaultAIJsonOptions(AIJsonOptions o)
    {
        o.JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        o.JsonSchemaExporterOptions = new JsonSchemaExporterOptions
        {
            TreatNullObliviousAsNonNullable = true,
            TransformSchemaNode = (context, schema) =>
            {
                var attributeProvider =
                    context.PropertyInfo is not null
                        ? context.PropertyInfo.AttributeProvider
                        : context.TypeInfo.Type;

                var descriptionAttr = attributeProvider?
                    .GetCustomAttributes(typeof(DescriptionAttribute), inherit: true)
                    .OfType<DescriptionAttribute>()
                    .FirstOrDefault();

                if (descriptionAttr is null)
                    return schema;

                var obj = schema as JsonObject ?? new JsonObject();
                obj["description"] = descriptionAttr.Description;
                return obj;
            }
        };
    }
}
