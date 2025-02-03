using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AutomationTestingProgram.Core;

[Authorize]
public class TestHub : Hub
{
    // Email -> Groups
    public static readonly ConcurrentDictionary<string, HashSet<string>> _userGroups = new ConcurrentDictionary<string, HashSet<string>>();
    // Email -> Connection ID
    public static readonly ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();


    // When user connects (or re-connects)
    public override async Task OnConnectedAsync()
    {
        string username = Context.User!.Identity!.Name!;
        string email = Context.User!.FindFirst("preferred_username")!.Value;

        _userConnections[email] = Context.ConnectionId;
        if (_userGroups.ContainsKey(email))
        {
            foreach (var testRunID in _userGroups[email])
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, testRunID);
            }
        }

        await Clients.Caller.SendAsync("OnConnected", $"User: {username} with Connection ID: {Context.ConnectionId} has connected to SignalR");
        await base.OnConnectedAsync();
    }

    // When user disconnects
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string email = Context.User!.FindFirst("preferred_username")!.Value;

        _userConnections.TryRemove(email, out _);
        if (_userGroups.ContainsKey(email))
        {
            foreach (var testRunID in _userGroups[email])
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, testRunID);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
