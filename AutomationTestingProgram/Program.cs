using AutomationTestingProgram.Core;
using Autofac.Extensions.DependencyInjection;
using AutomationTestingProgram.Configurations;
using AutomationTestingProgram.Modules.TestRunner.Services.Playwright.Executor;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

SecurityConfiguration.Configure(builder);
CultureConfiguration.Configure(builder);
LoggingConfiguration.Configure(builder);
AppSettingsConfiguration.Configure(builder);
FileUploadConfiguration.Configure(builder);
SwaggerConfiguration.Configure(builder);
DatabaseConfiguration.Configure(builder);
OtherConfiguration.Configure(builder);
HttpClientConfiguration.Configure(builder);
ServicesConfiguration.Configure(builder);

var app = builder.Build();

ApplicationLifetimeConfiguration.Configure(app);

MiddlewareConfiguration.Configure(app, builder);

PlaywrightExecutor.InitializeStaticVariables(app.Services.GetAutofacRoot());

app.MapControllers();
app.MapHub<TestHub>("/testHub");
app.MapFallbackToFile("index.html");

app.Run();