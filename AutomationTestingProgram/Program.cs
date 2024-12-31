using System.Net.WebSockets;
using System.Text;
using DotNetEnv;
using AutomationTestingProgram.Models;
using AutomationTestingProgram.Services;
using AutomationTestingProgram.Services.Logging;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using Microsoft.Identity.Web;
using AutomationTestingProgram.Models.Settings;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args); // builder used to configure services and middleware

DotNetEnv.Env.Load();

ConfigureServices(builder);

var app = builder.Build(); // represents configured web app

ConfigureApplicationLifetime(app);

ConfigureMiddleware(app);

app.Run();

void ConfigureServices(WebApplicationBuilder builder)
{
    // Authentication Setup (AAD)
    ConfigureAuthentication(builder);

    // Culture Setup
    ConfigureCulture(builder);

    // Logging Setup
    ConfigureLogging(builder);

    // Settings Setup
    ConfigureAppSettings(builder);

    // File Upload Setup
    ConfigureFileUpload(builder);

    // Services Setup
    RegisterServices(builder);

    // Controllers + other stuff
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    // AAD Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
}

void ConfigureCulture(WebApplicationBuilder builder)
{
    // Currently only en-CA
    var cultureConfig = builder.Configuration.GetSection("Culture");
    var defaultCulture = cultureConfig["Default"];
    var supportedCultures = cultureConfig.GetValue<string[]>("Supported");
    var supportedCultureInfo = supportedCultures!.Select(c => new CultureInfo(c)).ToList();

    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        options.DefaultRequestCulture = new RequestCulture(defaultCulture);
        options.SupportedCultures = supportedCultureInfo;
        options.SupportedUICultures = supportedCultureInfo;

        options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
    });
}

void ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();
    builder.Logging.AddProvider(new CustomLoggerProvider(LogManager.GetRunFolderPath()));
}

void ConfigureAppSettings(WebApplicationBuilder builder)
{
    // Configuring models with data from appsettings.json
    builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevops"));
    builder.Services.Configure<AzureKeyVaultSettings>(builder.Configuration.GetSection("AzureKeyVault"));
    builder.Services.Configure<MicrosoftGraphSettings>(builder.Configuration.GetSection("MicrosoftGraph"));
    builder.Services.Configure<RequestSettings>(builder.Configuration.GetSection("Request"));
    builder.Services.Configure<BrowserSettings>(builder.Configuration.GetSection("Browser"));
    builder.Services.Configure<ContextSettings>(builder.Configuration.GetSection("Context"));
    builder.Services.Configure<PageSettings>(builder.Configuration.GetSection("Page"));
}

void ConfigureFileUpload(WebApplicationBuilder builder)
{
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 15 * 1024 * 1024; // 15 MB limit total
        options.ValueLengthLimit = 10 * 1024 * 1024; // 10 MB limit per individual file
        options.MultipartHeadersCountLimit = 100; // Limit the number of headers
    });
}

void RegisterServices(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<WebSocketLogBroadcaster>();
    builder.Services.AddHttpClient("HttpClient", client =>
    {
        client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "WebAutomationTestingFramework/1.0");
    });
    builder.Services.AddSingleton<AzureKeyVaultService>();
    builder.Services.AddSingleton<PasswordResetService>();

    builder.Services.AddSingleton<ShutDownService>();
}

void ConfigureApplicationLifetime(WebApplication app)
{
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    var myService = app.Services.GetRequiredService<ShutDownService>();
    lifetime.ApplicationStopping.Register(myService.OnApplicationStopping);
}

void ConfigureMiddleware(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts(); // (HTTP Strict Transport Security) browsers interact with server only over HTTPS
    }
    else
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection(); // client and server comms encrypted
    app.UseStaticFiles(); // can request static assets for frontend

    app.UseRequestLocalization();
    app.UseMiddleware<RequestMiddleware>();

    app.UseWebSockets();
    WebSocketHandling(app);

    app.UseRouting(); // adds routing capabilities

    app.UseAuthentication(); // Add authentication middleware
    app.UseAuthorization(); // Add authorization middleware

    app.MapControllers();

    app.MapFallbackToFile("index.html"); // fallback to index.html for SPA routes
}

void WebSocketHandling(WebApplication app)
{
    app.Use(async (context, next) =>
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var path = context.Request.Path;
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            if (path == "/ws/logs")
            {
                await HandleLogsCommunication(webSocket, context);
            }
            else
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Invalid endpoint.", CancellationToken.None);
            }
        }
        else
        {
            await next();
        }
    });
}

async Task HandleLogsCommunication(WebSocket webSocket, HttpContext context)
{
    var broadcaster = context.RequestServices.GetRequiredService<WebSocketLogBroadcaster>();
    broadcaster.AddClient(webSocket);

    try
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client.", CancellationToken.None);
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
        throw;
    }
}
