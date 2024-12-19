using System.Net.WebSockets;
using System.Text;

namespace AutomationTestingProgram.Services;

public class WebSocketTaskBroadcaster
{
    private readonly List<WebSocket> _clients = new List<WebSocket>();

    public void AddClient(WebSocket client)
    {
        _clients.Add(client);
    }

    public void RemoveClient(WebSocket client)
    {
        _clients.Remove(client);
    }

    public async Task BroadcastMessage(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tasks = _clients
            .Where(c => c.State == WebSocketState.Open)
            .Select(client => client.SendAsync(new ArraySegment<byte>(messageBytes, 0, messageBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None));
        
        await Task.WhenAll(tasks);
    }
}