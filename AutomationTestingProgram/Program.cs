using System.Net.WebSockets;
using System.Text;
using AutomationTestingProgram.ModelsOLD;
using AutomationTestingProgram.Services;
using AutomationTestingProgram.Services.Logging;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

/*
 * Maybe add request limiting here instead
 * 
 */

var builder = WebApplication.CreateBuilder(args); // builder used to configure services and middleware

builder.Logging.ClearProviders();
builder.Logging.AddProvider(new CustomLoggerProvider(LogManager.GetRunFolderPath()));

builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevops"));
builder.Services.AddSingleton<WebSocketLogBroadcaster>();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 15 MB limit total
    options.ValueLengthLimit = 10 * 1024 * 1024; // 10 MB limit per individual file
    options.MultipartHeadersCountLimit = 100; // Limit the number of headers
});

builder.Services.AddControllers();
builder.Services.AddSingleton<CustomService>();

var app = builder.Build(); // represents configured web app

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var myService = app.Services.GetRequiredService<CustomService>();
lifetime.ApplicationStopping.Register(myService.OnApplicationStopping);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // (HTTP Strict Transport Security) browsers interact with server only over HTTPS
}

app.UseHttpsRedirection(); // client and server comms encrypted
app.UseStaticFiles(); // can request static assets for frontend

app.UseWebSockets();

app.Use(async (HttpContext context, Func<Task> next) =>
{
    if (context.Request.Path == "/ws/logs" && context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var broadcaster = app.Services.GetRequiredService<WebSocketLogBroadcaster>();

        broadcaster.AddClient(webSocket);

        try
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                Console.WriteLine("WebSocket Opened");
                await Task.Delay(1000);
                
                var res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (res.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client.", CancellationToken.None);
                }
            }
            Console.WriteLine("WebSocket Closed");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            broadcaster.RemoveClient(webSocket);
        }
    }
    else if (context.Request.Path == "/ws/desktop" && context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine("Electron WebSocket connected.");

        try
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (res.MessageType == WebSocketMessageType.Text || res.MessageType == WebSocketMessageType.Binary)
                {
                    var responseMsg = Encoding.UTF8.GetBytes("Message received by ASP.NET server.");
                    await webSocket.SendAsync(new ArraySegment<byte>(responseMsg), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                
                if (res.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client.", CancellationToken.None);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    else
    {
        await next();
    }
});

app.UseRouting(); // adds routing capabilities

app.MapControllers();

app.MapFallbackToFile("index.html"); // fallback to index.html for SPA routes

app.Run();
