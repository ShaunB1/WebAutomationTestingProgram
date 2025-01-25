using System.Net.WebSockets;
using System.Text;
using AutomationTestingProgram.Actions;
using DotNetEnv;
using AutomationTestingProgram.Models;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args); // builder used to configure services and middleware

string tenantId = builder.Configuration["AzureAd:TenantId"];
string clientId = builder.Configuration["AzureAd:ClientId"];

// AAD Authentication boilerplate
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// SignalR requires custom authorization options
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        { 
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/testHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

DotNetEnv.Env.Load();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevops"));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 31457280;
});
builder.Services.AddSingleton<WebSocketLogBroadcaster>();
builder.Services.AddScoped<AzureKeyVaultService>();
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddScoped<TestHub>();

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                    {
                        { $"api://{clientId}/.default", "Access your API" }
                    }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "oauth2"
                    }
                },
                new[] { $"api://{clientId}/.default" }
            }
        });
});

var app = builder.Build(); // represents configured web app

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => { 
        c.OAuthClientId(clientId); 
    });  
} 
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // (HTTP Strict Transport Security) browsers interact with server only over HTTPS
}

app.UseHttpsRedirection(); // client and server comms encrypted
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
    RequestPath = "/static"
});

app.UseRouting(); // adds routing capabilities

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TestHub>("/testHub");

app.MapFallbackToFile("index.html"); // fallback to index.html for SPA routes

app.Run();
