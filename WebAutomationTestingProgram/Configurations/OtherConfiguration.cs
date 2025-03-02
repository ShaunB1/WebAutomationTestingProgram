using WebAutomationTestingProgram.Infrastructure.ExternalApis;

namespace WebAutomationTestingProgram.Configurations;

public class OtherConfiguration
{
    public static void Configure(WebApplicationBuilder builder)
    {
        ConfigureExternalApis(builder);
    }

    private static void ConfigureExternalApis(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<AiService>();
        builder.Services.AddSignalR();
    }
}