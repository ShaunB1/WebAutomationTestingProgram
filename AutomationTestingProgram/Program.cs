using AutomationTestingProgram.Actions;
using AutomationTestingProgram.Infrastructure.Database;
using AutomationTestingProgram.Models;
using AutomationTestingProgram.Modules.AIConnector.Services;
using AutomationTestingProgram.Modules.DBConnector;
using DocumentFormat.OpenXml.InkML;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Web;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Diagnostics;
using AutomationTestingProgram.Core;
using AutomationTestingProgram.Modules.TestRunnerModule;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using System.Runtime;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;
using AutomationTestingProgram.Actions;
using AutomationTestingProgram.Modules.TestRunner.Services.Playwright.Executor;

var builder = WebApplication.CreateBuilder(args); // builder used to configure services and middleware

// GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

ConfigureServices(builder);

var app = builder.Build(); // represents configured web app

ConfigureApplicationLifetime(app);

ConfigureMiddleware(app);

PlaywrightExecutor.InitializeStaticVariables(app.Services.GetAutofacRoot());

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

    // Controllers + other stuff
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<CustomActionFilter>();
    });
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod() 
                .AllowAnyHeader();
        });
    });
    
    string tenantId = builder.Configuration["AzureAd:TenantId"];
    string clientId = builder.Configuration["AzureAd:ClientId"];
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

    builder.Services.AddSignalR(); // SignalR -> Websockets
    
    builder.Services.AddDbContext<AtpDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDB"))
    );
    builder.Services.AddDBConnectorModule();

    builder.Services.AddHttpClient<AiService>();

    // HttpClient -- NOTE: Must inject IHttpClientFactory to use
    builder.Services.AddHttpClient("HttpClient", client =>
    {
        client.Timeout = TimeSpan.FromMinutes(5); // 5 minute timeout
        client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "WebAutomationTestingFramework/1.0");
    });

    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        // Services Setup
        RegisterServices(containerBuilder);
    });

}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    // AAD Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

    string tenantId = builder.Configuration["AzureAd:TenantId"];
    string clientId = builder.Configuration["AzureAd:ClientId"];

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
}

void ConfigureCulture(WebApplicationBuilder builder)
{
    // Currently only en-CA
    var cultureConfig = builder.Configuration.GetSection("Culture");
    var defaultCulture = cultureConfig["Default"];
    var supportedCultures = cultureConfig.GetSection("Supported").Get<string[]>();
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
    builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Critical);
    builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Critical);
    builder.Logging.AddFilter("WebSocket*", LogLevel.Critical);

    // Suppress Microsoft.IdentityModel logs at INFO level
    builder.Logging.AddFilter("Microsoft.IdentityModel", LogLevel.Critical);
}

void ConfigureAppSettings(WebApplicationBuilder builder)
{
    // Configuring models with data from appsettings.json

    // Azure
    builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevops"));
    builder.Services.Configure<AzureKeyVaultSettings>(builder.Configuration.GetSection("AzureKeyVault"));
    
    // Other
    builder.Services.Configure<MicrosoftGraphSettings>(builder.Configuration.GetSection("MicrosoftGraph"));
    builder.Services.Configure<IOSettings>(builder.Configuration.GetSection("IO"));
    builder.Services.Configure<PathSettings>(builder.Configuration.GetSection("PATHS"));

    // Request
    builder.Services.Configure<RequestSettings>(builder.Configuration.GetSection("Request"));

    // Playwright
    builder.Services.Configure<PlaywrightSettings>(builder.Configuration.GetSection("Playwright"));
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

void RegisterServices(ContainerBuilder builder)
{
    // CORE

    builder.Register(c =>
    {
        return new CustomLoggerProvider(LogManager.GetRunFolderPath());
    })
    .As<ICustomLoggerProvider>()
    .SingleInstance();

    builder.RegisterType<ShutDownService>().SingleInstance();
    builder.RegisterType<RequestHandler>().SingleInstance();

    // TESTRUNNER MODULE

    builder.RegisterType<AzureKeyVaultService>().SingleInstance();
    builder.RegisterType<PasswordResetService>().SingleInstance();
    builder.RegisterType<CSVEnvironmentGetter>().SingleInstance();

    builder.RegisterType<PlaywrightObject>().SingleInstance();
    builder.RegisterType<BrowserFactory>().As<IBrowserFactory>().SingleInstance();
    builder.RegisterType<ContextFactory>().As<IContextFactory>().SingleInstance();
    builder.RegisterType<PageFactory>().As<IPageFactory>().SingleInstance();

    builder.RegisterType<ReaderFactory>().As<IReaderFactory>().SingleInstance();
    builder.RegisterType<ExecutorFactory>().As<IPlaywrightExecutorFactory>().SingleInstance();

    // ACTIONS (only those that need DI)
    builder.RegisterType<Login>().InstancePerDependency();
    builder.RegisterType<RunPrSQLScriptDelete>().InstancePerDependency();
    builder.RegisterType<RunPrSQLScriptRevert>().InstancePerDependency();

}

void ConfigureApplicationLifetime(WebApplication app)
{
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    var myService = app.Services.GetRequiredService<ShutDownService>();

    lifetime.ApplicationStopping.Register(() => myService.OnApplicationStopping().GetAwaiter().GetResult());
}

void ConfigureMiddleware(WebApplication app)
{
    app.UseCors("AllowSpecificOrigin");
    
    // Handles error responses
    app.Use(async (context, next) =>
    {
        await next();

        if (context.Response.HasStarted)
            return;

        var errorMessages = new Dictionary<int, string>
        {
            { 401, "Unauthorized access. Please provide a valid token." },
            { 403, "Forbidden access. You do not have permission." },
            { 400, "Bad request. Please check your input." },
            { 404, "Resource not found." },
            { 405, "Method not allowed." }
        };

        if (errorMessages.ContainsKey(context.Response.StatusCode))
        {
            var errorMessage = new { message = errorMessages[context.Response.StatusCode] };
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorMessage));
        }
    });

    // This is used to catch unhandled exceptions. These are logged as cirical (problem with pipeline)
    app.UseExceptionHandler(options =>
    {
        options.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var errorMessage = new { message = "An unexpected error occurred. Please try again later." };
            
            if (exception != null)
            {
                var customLogger = context.RequestServices.GetRequiredService<ILogger>();
                customLogger.LogCritical($"An unexpected error occured. Exception: {exception.Message}\n{exception.StackTrace}");
            }

            string json = JsonSerializer.Serialize(errorMessage);
            await context.Response.WriteAsync(json);
        });
    });


    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts(); // (HTTP Strict Transport Security) browsers interact with server only over HTTPS
    }
    else
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        string clientId = builder.Configuration["AzureAd:ClientId"];
        app.UseSwaggerUI(c => {
            c.OAuthClientId(clientId);
        });
    }

    app.UseHttpsRedirection(); // client and server comms encrypted
    app.UseStaticFiles(); // can request static assets for frontend
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Core", "StaticFiles", "Public")),
        RequestPath = "/static"
    });

    app.UseRequestLocalization();

    app.UseResponseCaching();

    app.UseRouting(); // adds routing capabilities

    app.UseMiddleware<RequestMiddleware>(); // needs routing

    app.UseAuthentication(); // Add authentication middleware
    app.UseAuthorization(); // Add authorization middleware

    app.MapControllers();
    app.MapHub<TestHub>("/testHub");

    app.MapFallbackToFile("index.html"); // fallback to index.html for SPA routes
}
