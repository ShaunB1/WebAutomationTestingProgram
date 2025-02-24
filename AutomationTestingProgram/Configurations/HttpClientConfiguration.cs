namespace AutomationTestingProgram.Configurations;

public class HttpClientConfiguration
{
    public static void Configure(WebApplicationBuilder builder)
    {
        ConfigureHttpClient(builder);
    }

    private static void ConfigureHttpClient(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("HttpClient", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "WebAutomationTestingFramework/1.0");
        });
    }
}