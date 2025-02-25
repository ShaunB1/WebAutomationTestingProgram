﻿using AutomationTestingProgram.Core;
using AutomationTestingProgram.Core.Settings;
using AutomationTestingProgram.Core.Settings.Playwright;
using AutomationTestingProgram.Core.Settings.Request;

namespace AutomationTestingProgram.Configurations;

public static class AppSettingsConfiguration
{
    public static void Configure(WebApplicationBuilder builder)
    {
        ConfigureAppSettings(builder);
    }

    private static void ConfigureAppSettings(WebApplicationBuilder builder)
    {
        builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevops"));
        builder.Services.Configure<AzureKeyVaultSettings>(builder.Configuration.GetSection("AzureKeyVault"));

        builder.Services.Configure<MicrosoftGraphSettings>(builder.Configuration.GetSection("MicrosoftGraph"));
        builder.Services.Configure<IOSettings>(builder.Configuration.GetSection("IO"));
        builder.Services.Configure<PathSettings>(builder.Configuration.GetSection("Paths"));

        builder.Services.Configure<RequestSettings>(builder.Configuration.GetSection("Request"));

        builder.Services.Configure<PlaywrightSettings>(builder.Configuration.GetSection("Playwright"));
        builder.Services.Configure<BrowserSettings>(builder.Configuration.GetSection("Browser"));
        builder.Services.Configure<ContextSettings>(builder.Configuration.GetSection("Context"));
        builder.Services.Configure<PageSettings>(builder.Configuration.GetSection("Page"));
    }
}