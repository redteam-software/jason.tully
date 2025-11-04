using Newtonsoft.Json.Linq;

namespace RedTeamGoCli.Services;

[RegisterSingleton<IRedTeamPlatformService>]
public class RedTeamPlatformService : IRedTeamPlatformService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PlatformConfiguration _configuration;
    private readonly string _baseUri = "https://dev.auth.redteam.com";

    public RedTeamPlatformService(IHttpClientFactory httpClientFactory, GoProjectConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration.PlatformConfiguration;
        _baseUri = _configuration.ApiBaseUrl ?? _baseUri;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var client = GetClient();

        if (string.IsNullOrEmpty(_configuration.ApiSecret))
        {
            "API Secret is not configured.".Error().WriteLine();
            return default;
        }

        if (string.IsNullOrEmpty(_configuration.ApiClientId))
        {
            "ApiClientId is not configured.".Error().WriteLine();
            return default;
        }

        var secret = _configuration.ApiSecret;
        var clientId = _configuration.ApiClientId;

        var body = new Dictionary<string, string>
        {
            { "client_id",clientId },
            { "client_secret",secret},
            { "grant_type","client_credentials" },
        };

        var formContent = new FormUrlEncodedContent(body);

        var httpResponse = await client.PostAsync("/connect/token", formContent);

        if (httpResponse.IsSuccessStatusCode)
        {
            var content = await httpResponse.Content.ReadAsStringAsync();

            $"Got Access Token".Success().WriteLine();
            JObject.Parse(content).TryGetValue("access_token", out var accessToken);

            return accessToken?.ToString();
        }
        else
        {
            Console.WriteLine($"Error: {httpResponse.StatusCode}");
            return default;
        }
    }

    private HttpClient GetClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_baseUri);
        return client;
    }


    public async Task SignupAsync(string accessToken, string email, string applicationName)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://dev.api.redteam.com");

        var requestUri = $"/v1/users/signup?applicationName={Uri.EscapeDataString(applicationName)}";

        var payload = new
        {
            applicationName = applicationName,
            emailAddress = email,
            // Uncomment and populate these fields if needed in the future:
            // legacyCompanyId = companyId,
            // legacyUserId = contactId,
            legacyUserName = email,
            firstName = "", // Provide actual first name if available
            lastName = ""   // Provide actual last name if available
        };

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request);

        var responseString = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var model = System.Text.Json.JsonSerializer.Deserialize<SignupResponse>(responseString);
            $"Successfully signed up {model!.userId.NumericValue()}.".WriteLine();
        }
        else
        {
            $"Failed to sign up user: {response.StatusCode}".WriteLine();
            Console.WriteLine(responseString);
        }
    }



    public class SignupResponse
    {
        public int userId { get; set; }
        public string emailAddress { get; set; } = null!;
        public object cognitoUserId { get; set; } = null!;
        public object cognitoUserName { get; set; } = null!;
        public string applicationName { get; set; } = null!;
        public object legacyUserId { get; set; } = null!;
        public string legacyUserName { get; set; } = null!;
    }
}