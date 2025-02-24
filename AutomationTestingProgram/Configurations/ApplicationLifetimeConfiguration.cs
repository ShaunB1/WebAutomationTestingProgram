using AutomationTestingProgram.Core.Services.ApplicationLifetime;

namespace AutomationTestingProgram.Configurations;

public static class ApplicationLifetimeConfiguration
{
    public static void Configure(WebApplication app)
    {
        ConfigureApplicationLifetime(app);
    }

    private static void ConfigureApplicationLifetime(WebApplication app)
    {
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        var myService = app.Services.GetRequiredService<ShutDownService>();

        lifetime.ApplicationStopping.Register(() => myService.OnApplicationStopping().GetAwaiter().GetResult());
    }
}