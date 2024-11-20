using System.Net.WebSockets;
using System.Text;
using DotNetEnv;
using AutomationTestingProgram.Models;
using AutomationTestingProgram.Services;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebSockets;

var builder = WebApplication.CreateBuilder(args); // builder used to configure services and middleware

DotNetEnv.Env.Load();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevops"));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 31457280;
});
builder.Services.AddSingleton<WebSocketLogBroadcaster>();
builder.Services.AddControllers();

var app = builder.Build(); // represents configured web app

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // (HTTP Strict Transport Security) browsers interact with server only over HTTPS
}

app.UseHttpsRedirection(); // client and server comms encrypted
app.UseStaticFiles(); // can request static assets for frontend

app.UseWebSockets();

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
        else if (path == "/ws/recorder")
        {
            await HandleRecorderCommunication(webSocket, context);
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

app.UseRouting(); // adds routing capabilities

app.MapControllers();

app.MapFallbackToFile("index.html"); // fallback to index.html for SPA routes

app.Run();

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

async Task HandleRecorderCommunication(WebSocket webSocket, HttpContext context)
{
    
}

async Task HandleWebSocket(HttpContext context, Func<WebSocketMiddleware, IServiceProvider, Task> communicationHandler)
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        // await communicationHandler(webSocket, context.RequestServices);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
}

async Task HandleLogsWebSocket(HttpContext context)
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
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
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client.",
                        CancellationToken.None);
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
        finally
        {
            broadcaster.RemoveClient(webSocket);
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
}

async Task HandleRecorderWebSocket(HttpContext context)
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        while (webSocket.State == WebSocketState.Open)
        {
            
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
}
