using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Web.Http;

[Authorize]
public class TestHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> _connections = new ConcurrentDictionary<string, string>();

    public async Task AddClient(string testRunId)
    {
        string username = Context.User.Identity.Name;
        _connections[Context.ConnectionId] = testRunId;
        Console.WriteLine($"User: {username} with Connection ID: {Context.ConnectionId} has joined Test Run: {testRunId}");
        await Clients.Group(testRunId).SendAsync("AddClient", $"User: {username} with Connection ID: {Context.ConnectionId} has joined Test Run: {testRunId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string username = Context.User.Identity.Name;
        if (_connections.TryRemove(Context.ConnectionId, out var testRunId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, testRunId);
            Console.WriteLine($"User: {username} with Connection ID: {Context.ConnectionId} has disconnected from Test Run: {testRunId}");
            await Clients.Group(testRunId).SendAsync("AddClient", $"User: {username} with Connection ID: {Context.ConnectionId} has joined Test Run: {testRunId}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
