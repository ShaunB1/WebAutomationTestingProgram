using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace AutomationTestingProgram.Services;

public class WebSocketRecorderHandler
{
    private readonly ConcurrentBag<WebSocket> _clients = new();

    public void AddClient(WebSocket client)
    {
        _clients.Add(client);
    }

    // public void RemoveClient(WebSocket client)
    // {
    //     _clients.Remove(client);
    // }

    public void ProcessMessage(string message)
    {
        
    }
}