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
builder.Services.Configure<AzureKeyVaultSettings>(builder.Configuration.GetSection("AzureKeyVault"));
builder.Services.Configure<KeychainFileSettings>(builder.Configuration.GetSection("KeychainFile"));
builder.Services.Configure<MicrosoftGraphSettings>(builder.Configuration.GetSection("MicrosoftGraph"));
builder.Services.AddSingleton<WebSocketLogBroadcaster>();
builder.Services.AddSingleton<WebSocketRecorderHandler>();
builder.Services.AddScoped<AzureKeyVaultService>();
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddScoped<KeychainFileSettings>();
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
    var recorderHandler = context.RequestServices.GetRequiredService<WebSocketRecorderHandler>();
    var clientId = recorderHandler.AddClient(webSocket);

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
            else if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received from recorder client {clientId}: {message}");
                recorderHandler.ProcessMessage(message);

                var responseMessage = "Step recorded";
                var responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true,
                    CancellationToken.None);
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
        recorderHandler.RemoveClient(clientId);
    }
}
