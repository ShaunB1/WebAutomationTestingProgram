﻿using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace AutomationTestingProgram.Services;

public class WebSocketLogBroadcaster
{
    private readonly ConcurrentBag<WebSocket> _clients = new();

    public void AddClient(WebSocket client)
    {
        _clients.Add(client);
        Console.WriteLine("Successfully added websocket client.");
    }

    public async Task BroadcastLogAsync(string logMessage)
    {
        var buffer = Encoding.UTF8.GetBytes(logMessage);
        var segment = new ArraySegment<byte>(buffer);

        foreach (var client in _clients)
        {
            if (client.State == WebSocketState.Open)
            {
                await client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                Console.WriteLine("Broadcasted log.");
            }
            else
            {
                Console.WriteLine("Websocket is not open.");
            }
        }
    }

    public void RemoveClient(WebSocket client)
    {
        _clients.TryTake(out client);
        Console.WriteLine("Successfully removed websocket client.");
    }
}