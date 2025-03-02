using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace WebAutomationTestingProgram.Configurations;

public static class CultureConfiguration
{
    public static void Configure(WebApplicationBuilder builder)
    {
        ConfigureCulture(builder);
    }

    private static void ConfigureCulture(WebApplicationBuilder builder)
    {
        var cultureConfig = builder.Configuration.GetSection("Culture");
        var defaultCulture = cultureConfig["Default"];
        var supportedCultures = cultureConfig.GetSection("Supported").Get<List<string>>();
        var supportedCultureInfo = supportedCultures!.Select(c => new CultureInfo(c)).ToList();

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
            options.SupportedCultures = supportedCultureInfo;
            options.SupportedUICultures = supportedCultureInfo;
            options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
        });
    }
}