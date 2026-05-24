using System.Globalization;
using System.Text.Json.Nodes;

namespace ParserMock;

public static class ManualRawPostingFactory
{
    private const string DefaultSource = "tg";

    public static bool TryCreate(
        RawPostingRequest request,
        out JsonObject posting,
        out string? error)
    {
        posting = [];

        if (string.IsNullOrWhiteSpace(request.RawText))
        {
            error = "raw_text is required.";
            return false;
        }

        var source = string.IsNullOrWhiteSpace(request.Source)
            ? DefaultSource
            : request.Source.Trim();
        var url = string.IsNullOrWhiteSpace(request.Url)
            ? CreateManualTelegramUrl()
            : request.Url.Trim();
        var postedAt = string.IsNullOrWhiteSpace(request.PostedAt)
            ? FormatPostedAt(DateTimeOffset.UtcNow)
            : request.PostedAt.Trim();

        posting["source"] = source;
        posting["posted_at"] = postedAt;
        posting["url"] = url;
        posting["payload"] = request.Payload?.DeepClone();
        posting["raw_text"] = request.RawText.Trim();

        error = null;
        return true;
    }

    private static string CreateManualTelegramUrl()
    {
        return $"https://t.me/parsermock_manual/{Guid.NewGuid():N}";
    }

    private static string FormatPostedAt(DateTimeOffset value)
    {
        return value.UtcDateTime.ToString("yy:MM:dd:HH:mm:ss", CultureInfo.InvariantCulture);
    }
}
