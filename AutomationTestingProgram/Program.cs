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

var builder = WebApplication.CreateBuilder(args); // builder used to configure services and middleware

// AAD Authentication boilerplate
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

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

app.UseHttpsRedirection(); // client and server comms encrypted
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
    RequestPath = "/static"
});

app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var path = context.Request.Path;

        if (path == "/ws/logs")
        {
            var testRunId = context.Request.Query["testRunId"].ToString();

            if (string.IsNullOrEmpty(testRunId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid test run ID.");
                return;
            }
            
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            
            await HandleLogsCommunication(webSocket, context, testRunId);
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Endpoint not found.");
        }
    }
    else
    {
        await next();
    }
});

app.UseRouting(); // adds routing capabilities

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html"); // fallback to index.html for SPA routes

app.Run();

async Task HandleLogsCommunication(WebSocket webSocket, HttpContext context, string testRunId)
{
    var broadcaster = context.RequestServices.GetRequiredService<WebSocketLogBroadcaster>();
    var clientId = broadcaster.AddClient(webSocket, testRunId);
    var clientIdMessage = Encoding.UTF8.GetBytes($"ClientId: {clientId}");

    await webSocket.SendAsync(new ArraySegment<byte>(clientIdMessage), WebSocketMessageType.Text, true, CancellationToken.None);
    
    try
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client.", CancellationToken.None);
                broadcaster.RemoveClient(clientId);
                Console.WriteLine($"Client {clientId} disconnected.");
            }
            else
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received from logs client: {message}");
            }
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine(exception);
        broadcaster.RemoveClient(clientId);
        throw;
    }
}
