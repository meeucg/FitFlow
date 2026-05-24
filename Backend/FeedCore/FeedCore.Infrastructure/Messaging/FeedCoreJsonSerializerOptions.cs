using System.Text.Json;
using System.Text.Json.Serialization;

namespace FeedCore.Infrastructure.Messaging;

public static class FeedCoreJsonSerializerOptions
{
    public static JsonSerializerOptions CreateSnakeCase()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
        return options;
    }
}
