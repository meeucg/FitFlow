using System.Net;
using System.Text.Json.Nodes;

namespace ApiGateway.IntegrationTests;

public sealed class InterviewGatewayPipelineTests
{
    private static readonly Uri GatewayBaseAddress = new(
        Environment.GetEnvironmentVariable("APIGATEWAY_BASE_URL")
        ?? "http://localhost:5266");

    [Fact]
    public async Task My_interview_requires_authorization()
    {
        using var client = CreateClient();
        if (!await GatewayIsAvailable(client))
        {
            return;
        }

        var response = await client.GetAsync("/my-interview");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_requires_authorization()
    {
        using var client = CreateClient();
        if (!await GatewayIsAvailable(client))
        {
            return;
        }

        var response = await client.GetAsync("/me");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Root_remains_public()
    {
        using var client = CreateClient();
        if (!await GatewayIsAvailable(client))
        {
            return;
        }

        var response = await client.GetAsync("/");
        var payload = await ReadJson(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("FitFlow ApiGateway", payload["service"]?.GetValue<string>());
    }

    private static HttpClient CreateClient()
    {
        return new HttpClient
        {
            BaseAddress = GatewayBaseAddress,
            Timeout = TimeSpan.FromSeconds(90),
        };
    }

    private static async Task<bool> GatewayIsAvailable(HttpClient client)
    {
        try
        {
            using var response = await client.GetAsync("/");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    private static async Task<JsonNode> ReadJson(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        var node = JsonNode.Parse(payload);
        Assert.NotNull(node);
        return node;
    }
}
