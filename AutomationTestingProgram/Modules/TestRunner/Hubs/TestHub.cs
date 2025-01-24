using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Web.Http;

[Authorize]
public class TestHub : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _connections = new ConcurrentDictionary<string, HashSet<string>>();
    public async Task AddClient(string testRunId)
    {
        string username = Context.User.Identity.Name;
        await Groups.AddToGroupAsync(Context.ConnectionId, testRunId);
        HashSet<string> groups;
        if (!_connections.TryGetValue(Context.ConnectionId, out groups))
        {
            groups = new HashSet<string>();
            _connections.TryAdd(Context.ConnectionId, groups);
        }
        groups.Add(Context.ConnectionId);
        Console.WriteLine($"User: {username} with Connection ID: {Context.ConnectionId} has joined Test Run: {testRunId}");
        await Clients.Group(testRunId).SendAsync("AddClient", $"User: {username} with Connection ID: {Context.ConnectionId} has joined Test Run: {testRunId}");
    }
    public async Task RemoveClient(string testRunId)
    {
        string username = Context.User.Identity.Name;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, testRunId);
        if (_connections.TryGetValue(Context.ConnectionId, out var groups))
        {
            groups.Remove(Context.ConnectionId);
        }
        Console.WriteLine($"User: {username} with Connection ID: {Context.ConnectionId} has left Test Run: {testRunId}");
        await Clients.Group(testRunId).SendAsync("AddClient", $"User: {username} with Connection ID: {Context.ConnectionId} has left Test Run: {testRunId}");
    }

    public override async Task OnConnectedAsync()
    {
        string username = Context.User.Identity.Name;
        Console.WriteLine($"User: {username} with Connection ID: {Context.ConnectionId} has connected to SignalR");
        await Clients.Caller.SendAsync("OnConnected", $"User: {username} with Connection ID: {Context.ConnectionId} has connected to SignalR");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string username = Context.User.Identity.Name;
        if (_connections.TryRemove(Context.ConnectionId, out var groups))
        {
            foreach (var testRunId in groups)
            {
                Console.WriteLine($"User: {username} with Connection ID: {Context.ConnectionId} has disconnected from Test Run: {testRunId}");
                await Clients.Group(testRunId).SendAsync("OnDisconnected", $"User: {username} with Connection ID: {Context.ConnectionId} has disconnected from Test Run: {testRunId}");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, testRunId);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}
