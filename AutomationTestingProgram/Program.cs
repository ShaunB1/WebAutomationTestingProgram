using System.Net.WebSockets;
using System.Text;
using AutomationTestingProgram.Models;
using AutomationTestingProgram.Services;

var builder = WebApplication.CreateBuilder(args); // builder used to configure services and middleware

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevops"));
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
    else
    {
        await next();
    }
});

app.UseRouting(); // adds routing capabilities

app.MapControllers();

app.MapFallbackToFile("index.html"); // fallback to index.html for SPA routes

app.Run();
