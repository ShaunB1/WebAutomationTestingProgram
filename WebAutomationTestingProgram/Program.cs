using System.Net.WebSockets;
using System.Text;
using DotNetEnv;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Microsoft.OpenApi.Models;
using WebAutomationTestingProgram.Core.Hubs;
using WebAutomationTestingProgram.Core.Hubs.Services;
using WebAutomationTestingProgram.Core.Settings.Azure;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Services;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Services.AzureReporter;

var builder = WebApplication.CreateBuilder(args); // builder used to configure services and middleware

// AAD Authentication boilerplate
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) &&
                context.HttpContext.WebSockets.IsWebSocketRequest)
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

DotNetEnv.Env.Load();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevops"));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 31457280;
});
builder.Services.AddSingleton<SignalRService>();
builder.Services.AddSingleton<WebSocketLogBroadcaster>();
builder.Services.AddScoped<AzureKeyVaultService>();
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddSignalR();

builder.Services.AddControllers();

string tenantId = builder.Configuration["AzureAd:TenantId"];
string clientId = builder.Configuration["AzureAd:ClientId"];

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
app.UseRouting(); // adds routing capabilities
app.UseWebSockets();
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection(); // client and server comms encrypted
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Core", "StaticFiles", "Public")),
    RequestPath = "/static"
});

app.MapHub<TestHub>("/testHub");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html"); // fallback to index.html for SPA routes

app.Run();