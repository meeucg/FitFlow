using System.Text.Json;

namespace ParserMock.Tests;

public sealed class PostingPoolLoaderTests
{
    [Fact]
    public void Load_reads_jsonl_and_json_array_as_one_pool()
    {
        using var directory = new TempDirectory();
        var jsonlPath = Path.Combine(directory.Path, "result.jsonl");
        var kworkPath = Path.Combine(directory.Path, "kwork.json");

        File.WriteAllText(
            jsonlPath,
            """
            {"source":"tg","posted_at":"26:05:18:15:24:00","url":"https://t.me/jobs/1","payload":null,"raw_text":"Need backend developer"}

            """);

        File.WriteAllText(
            kworkPath,
            """
            [
              {
                "source": "kwork",
                "posted_at": "2026:05:19:23:36:38",
                "url": "https://kwork.example/jobs/2",
                "payload": {
                  "author": "client",
                  "title": "Python task",
                  "price_min": 1000,
                  "price_max": 1000,
                  "description": "Build a script.",
                  "attached_files": []
                },
                "raw_text": "Python task\nBuild a script."
              }
            ]
            """);

        var postings = PostingPoolLoader.Load(jsonlPath, kworkPath);

        Assert.Equal(2, postings.Count);
        Assert.Contains(postings, posting => posting["source"]?.GetValue<string>() == "tg");
        Assert.Contains(postings, posting => posting["source"]?.GetValue<string>() == "kwork");
    }

    [Fact]
    public void Load_defaults_missing_payload_currency_to_rub()
    {
        using var directory = new TempDirectory();
        var jsonlPath = Path.Combine(directory.Path, "result.jsonl");
        var kworkPath = Path.Combine(directory.Path, "kwork.json");

        File.WriteAllText(jsonlPath, string.Empty);
        File.WriteAllText(
            kworkPath,
            """
            [
              {
                "source": "kwork",
                "posted_at": "2026:05:19:23:36:38",
                "url": "https://kwork.example/jobs/1",
                "payload": { "title": "Task", "attached_files": [] },
                "raw_text": "Task"
              }
            ]
            """);

        var posting = Assert.Single(PostingPoolLoader.Load(jsonlPath, kworkPath));
        var serialized = PostingPoolLoader.Serialize(posting);

        using var document = JsonDocument.Parse(serialized);
        Assert.Equal(
            "rub",
            document.RootElement.GetProperty("payload").GetProperty("currency").GetString());
    }

    [Fact]
    public void PickRandom_returns_posting_from_union_pool()
    {
        using var directory = new TempDirectory();
        var jsonlPath = Path.Combine(directory.Path, "result.jsonl");
        var kworkPath = Path.Combine(directory.Path, "kwork.json");

        File.WriteAllText(
            jsonlPath,
            """
            {"source":"tg","posted_at":"26:05:18:15:24:00","url":"https://t.me/jobs/1","payload":null,"raw_text":"Need backend developer"}
            """);
        File.WriteAllText(kworkPath, "[]");

        var postings = PostingPoolLoader.Load(jsonlPath, kworkPath);
        var selected = PostingPoolLoader.PickRandom(postings, new Random(123));

        Assert.Equal("https://t.me/jobs/1", selected["url"]?.GetValue<string>());
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "parsermock-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
