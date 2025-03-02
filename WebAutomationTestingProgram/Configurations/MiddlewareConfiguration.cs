using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using WebAutomationTestingProgram.Core.Middleware;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebAutomationTestingProgram.Configurations;

public class MiddlewareConfiguration
{
    public static void Configure(WebApplication app, WebApplicationBuilder builder)
    {
        ConfigureMiddleware(app, builder);
    }

    private static void ConfigureMiddleware(WebApplication app, WebApplicationBuilder builder)
    {
        app.UseCors("AllowSpecificOrigin");

        app.Use(async (context, next) =>
        {
            await next();

            if (context.Response.HasStarted) return;

            var errorMessages = new Dictionary<int, string>
            {
                { 401, "Unauthorized access. Please provide a valid token." },
                { 403, "Forbidden access. You do not have permission." },
                { 400, "Bad request. Please check your input." },
                { 404, "Resource not found." },
                { 405, "Method not allowed." }
            };

            if (errorMessages.TryGetValue(context.Response.StatusCode, out var value))
            {
                var errorMessage = new { message = value };
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(errorMessage));
            }
        });

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
                    customLogger.LogCritical($"An unexpected error occurred: Exception: {exception.Message}\n{exception.StackTrace}");
                }

                var json = JsonSerializer.Serialize(errorMessage);
                await context.Response.WriteAsync(json);
            });
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            
            var clientId = builder.Configuration["AzureAd:ClientId"];

            app.UseSwaggerUI(options => options.OAuthClientId(clientId));
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Core", "StaticFiles", "Public")),
            RequestPath = "/static"
        });
        
        app.UseRequestLocalization();
        app.UseResponseCaching();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseMiddleware<RequestMiddleware>();
    }
}