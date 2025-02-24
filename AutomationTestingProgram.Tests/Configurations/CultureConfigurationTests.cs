using AutomationTestingProgram.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Tests.Configurations;

public class CultureConfigurationTests
{
    private WebApplicationBuilder CreateBuilderWithConfiguration(Dictionary<string, string> configValues)
    {
        var builder = WebApplication.CreateBuilder();
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!);
        var configuration = configBuilder.Build();
        builder.Services.AddSingleton<IConfiguration>(configuration);

        return builder;
    }
    
    [Fact]
    public void Configure_ShouldSetDefaultCulture()
    {
        var configValues = new Dictionary<string, string>
        {
            { "Culture:Default", "en-CA" },
            { "Culture:Supported:0", "en-CA" },
        };
        var builder = CreateBuilderWithConfiguration(configValues);
        
        CultureConfiguration.Configure(builder);
        
        var services = builder.Services.BuildServiceProvider();
        var options = services.GetService<IOptions<RequestLocalizationOptions>>()?.Value;

        Assert.NotNull(options);
        Assert.Equal("en-CA", options.DefaultRequestCulture.Culture.Name);
    }

    [Fact]
    public void Configure_ShouldSetSupportedCultures()
    {
        var configValues = new Dictionary<string, string>
        {
            { "Culture:Default", "en-CA" },
            { "Culture:Supported:0", "en-CA" },
        };
        
        var builder = CreateBuilderWithConfiguration(configValues);
        var cultureConfig = builder.Configuration.GetSection("Culture");
        
        CultureConfiguration.Configure(builder);
        
        var services = builder.Services.BuildServiceProvider();
        var options = services.GetService<IOptions<RequestLocalizationOptions>>()?.Value;
        
        Assert.NotNull(options);
        Assert.Equal(1, options.SupportedCultures?.Count);
        Assert.Contains(options.SupportedCultures!, c => c.Name == "en-CA");
    }
}