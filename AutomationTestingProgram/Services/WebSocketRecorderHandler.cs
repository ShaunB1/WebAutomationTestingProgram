using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace AutomationTestingProgram.Services;

public class WebSocketRecorderHandler
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();

    public Guid AddClient(WebSocket client)
    {
        var clientId = Guid.NewGuid();
        _clients[clientId] = client;
        Console.WriteLine("Recorder websocket client added.");
        return clientId;
    }

    public void RemoveClient(Guid clientId)
    {
        _clients.TryRemove(clientId, out _);
        Console.WriteLine("Recorder websocket client removed.");
    }

    public void ProcessMessage(string message)
    {
        Console.WriteLine($"Processing recorded message: {message}");
    }
}