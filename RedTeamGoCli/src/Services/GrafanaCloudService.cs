using System.Text.Json;

namespace RedTeamGoCli.Services;

[RegisterSingleton<IGrafanaCloudService>]
internal class GrafanaCloudService : IGrafanaCloudService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GoProjectConfiguration _configuration;

    public GrafanaCloudService(IHttpClientFactory httpClientFactory, GoProjectConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task PostLokiLogMessageAsync(string logMessage, Dictionary<string, string>? tags = null)
    {
        var client = _httpClientFactory.CreateClient();
        var (apiToken, url, orgId) = _configuration.GrafanaCloudConfiguration;

        if (string.IsNullOrEmpty(apiToken) || string.IsNullOrEmpty(url) || string.IsNullOrEmpty(orgId))
        {
            throw new InvalidOperationException("Grafana Cloud Configuration is not set properly.");
        }

        var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{orgId}:{apiToken}"));
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        logMessage = JsonSerializer.Serialize(new
        {
            message = logMessage
        });

        var defaultTags = new Dictionary<string, string>
        {
            { "app", "redteamgo4" },
            { "environment", "dev" },
            { "level", "info" }
        };
        tags ??= new Dictionary<string, string>();

        var allTags = tags ?? defaultTags;

        allTags["level"] = "info";
        allTags["deployment_environment"] = "uat";

        var request = new LokiLogPublishRequest
        {
            streams = new[]
          {
                new Stream
                {
                    stream = allTags,
                    values = new List<List<string>>
                    {
                        new List<string>
                        {
                            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "000000", // nanoseconds
                            logMessage,
                        }
                    }
                }
            }
        };

        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{url}/loki/api/v1/push", content);

        if (response.IsSuccessStatusCode)
        {
            var c = await response.Content.ReadAsStringAsync();
            "Loki log published successfully.".WriteLine();
            Console.WriteLine(c);
        }
        else
        {
            $"Failed to publish Loki log: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}".WriteLine();
        }
    }

    public class LokiLogPublishRequest
    {
        public Stream[] streams { get; set; } = null!;
    }

    public class Stream
    {
        public Dictionary<string, string> stream { get; set; } = null!;
        public List<List<string>> values { get; set; } = null!;
    }
}