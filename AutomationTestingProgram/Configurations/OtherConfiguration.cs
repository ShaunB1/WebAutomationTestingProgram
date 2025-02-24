using AutomationTestingProgram.Modules.AIConnector.Services;

namespace AutomationTestingProgram.Configurations;

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