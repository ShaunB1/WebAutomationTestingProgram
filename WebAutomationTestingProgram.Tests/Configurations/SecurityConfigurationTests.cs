using WebAutomationTestingProgram.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebAutomationTestingProgram.Tests.Configurations;

public class SecurityConfigurationTests
{
    [Fact]
    public async Task ConfigureAuthentication_Should_Add_JwtBearerAuthentication()
    {
        var builder = WebApplication.CreateBuilder();

        SecurityConfiguration.Configure(builder);
        
        var serviceProvider = builder.Services.BuildServiceProvider();
        var authSchemeProvider = serviceProvider.GetService<IAuthenticationSchemeProvider>();

        Assert.NotNull(authSchemeProvider);
        var scheme = await authSchemeProvider.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme);
        Assert.NotNull(scheme);
    }

    [Fact]
    public void ConfigureCors_Should_Add_CorsPolicy()
    {
        var builder = WebApplication.CreateBuilder();
        
        SecurityConfiguration.Configure(builder);
        
        var serviceProvider = builder.Services.BuildServiceProvider();
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        
        Assert.NotNull(corsOptions);
        var policy = corsOptions.Value.GetPolicy("AllowSpecificOrigin");
        Assert.NotNull(policy);
        
        Assert.True(policy.AllowAnyHeader);
        Assert.True(policy.AllowAnyMethod);
        Assert.True(policy.AllowAnyOrigin);
    }
    
    [Fact]
    public void ConfigureAuthentication_Should_Configure_JwtBearerEvents()
    {
        var builder = WebApplication.CreateBuilder();
        
        SecurityConfiguration.Configure(builder);
        
        var serviceProvider = builder.Services.BuildServiceProvider();
        var jwtBearerOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);
        
        Assert.NotNull(jwtBearerOptions.Events);
        Assert.NotNull(jwtBearerOptions.Events.OnMessageReceived);
    }
}