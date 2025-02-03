using System.Text;
using System.Text.Json;


namespace AutomationTestingProgram.Modules.AIConnector.Services;

public class AiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"];
    }

    public async Task<string> GetResponseAsync(string message)
    {
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new []
            {
                new { role = "system", content = "You are an AI assistant." },
                new { role = "user", content = message }
            }
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"OpenAI API error: {responseBody}");
        }
        
        using var jsonDoc = JsonDocument.Parse(responseBody);
        return jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }
}