using AutomationTestingProgram.Core;
using AutomationTestingProgram.Core.Services.Logging;

namespace AutomationTestingProgram.Configurations;

public static class LoggingConfiguration
{
    public static void Configure(WebApplicationBuilder builder)
    {
        ConfigureLogging(builder);
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new CustomLoggerProvider(LogManager.GetRunFolderPath()));
        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Critical);
        builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Critical);
        builder.Logging.AddFilter("WebSocket*", LogLevel.Critical);
        builder.Logging.AddFilter("Microsoft.IdentityModel", LogLevel.Critical);
    }
}