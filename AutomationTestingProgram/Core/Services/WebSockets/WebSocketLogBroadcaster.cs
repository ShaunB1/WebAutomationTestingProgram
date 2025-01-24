using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace AutomationTestingProgram.Core;

public class WebSocketLogBroadcaster : TextWriter
{
    private readonly ConcurrentDictionary<string, WebSocket> _clients = new();
    private readonly ConcurrentDictionary<string, string> _testRuns = new();
    private readonly TextWriter original;

    public WebSocketLogBroadcaster()
    {
        original = Console.Out;
        Console.SetOut(this);
    }

    public string AddClient(WebSocket client, string testRunId)
    {
        var clientId = Guid.NewGuid().ToString();
        _clients.TryAdd(clientId, client);
        _testRuns.TryAdd(testRunId, clientId);

        Console.WriteLine($"Added client {clientId} for test run {testRunId}.");
        return clientId;
    }

    public async Task BroadcastLogAsync(string logMessage, string testRunId)
    {
        if (!_testRuns.TryGetValue(testRunId, out var clientId))
        {
            Console.WriteLine($"No client found for test run {testRunId}");
            return;
        }

        if (!_clients.TryGetValue(clientId, out var client))
        {
            Console.WriteLine($"No active WebSocket client found for client {clientId}");
            return;
        }

        if (client.State == WebSocketState.Open)
        {
            var payload = new
            {
                testRunId,
                logMessage
            };

            var jsonMessage = JsonSerializer.Serialize(payload);

            var buffer = Encoding.UTF8.GetBytes(jsonMessage);
            var segment = new ArraySegment<byte>(buffer);

            await client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            original.WriteLine(logMessage);
        }
        else
        {
            Console.WriteLine($"WebSocket for client {clientId} is not open. Removing client.");
            _clients.TryRemove(clientId, out _);
            _testRuns.TryRemove(testRunId, out _);
        }
    }

    public bool RemoveClient(string clientId)
    {
        if (_clients.TryRemove(clientId, out _))
        {
            Console.WriteLine($"Successfully removed websocket client {clientId}.");
            return true;
        }
        Console.WriteLine($"Failed to remove websocket client {clientId}.");
        return false;
    }

    public override void WriteLine(string s)
    {
        original.WriteLine(s);
    }

    public override Encoding Encoding => Encoding.UTF8;
}