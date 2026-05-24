using System.Text.Json;
using System.Text.Json.Nodes;

namespace ParserMock;

public static class PostingPoolLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static IReadOnlyList<JsonObject> Load(string telegramJsonlPath, string kworkJsonPath)
    {
        var postings = new List<JsonObject>();

        LoadJsonl(telegramJsonlPath, postings);
        LoadJsonArray(kworkJsonPath, postings);

        return postings;
    }

    public static string Serialize(JsonObject posting)
    {
        return posting.ToJsonString(SerializerOptions);
    }

    public static JsonObject PickRandom(
        IReadOnlyList<JsonObject> postings,
        Random random)
    {
        if (postings.Count == 0)
        {
            throw new ArgumentException("Posting pool must not be empty.", nameof(postings));
        }

        return postings[random.Next(postings.Count)];
    }

    private static void LoadJsonl(string path, ICollection<JsonObject> postings)
    {
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var posting = JsonNode.Parse(line)?.AsObject();
            if (posting is null)
            {
                continue;
            }

            NormalizePosting(posting);
            postings.Add(posting);
        }
    }

    private static void LoadJsonArray(string path, ICollection<JsonObject> postings)
    {
        var root = JsonNode.Parse(File.ReadAllText(path));
        if (root is not JsonArray array)
        {
            return;
        }

        foreach (var item in array)
        {
            if (item is not JsonObject posting)
            {
                continue;
            }

            NormalizePosting(posting);
            postings.Add(posting);
        }
    }

    private static void NormalizePosting(JsonObject posting)
    {
        if (posting["payload"] is JsonObject payload
            && payload["currency"] is null)
        {
            payload["currency"] = "rub";
        }
    }
}
