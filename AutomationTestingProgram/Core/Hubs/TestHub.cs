using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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
        string email = Context.User!.FindFirst("preferred_username")!.Value;

        _userConnections[email] = Context.ConnectionId;
        if (_userGroups.ContainsKey(email))
        {
            foreach (var testRunID in _userGroups[email])
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, testRunID);
            }
        }

        await Clients.Caller.SendAsync("OnConnected", $"User: {email} has connected to SignalR");
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

    public async Task AddClient(string testRunId)
    {
        string email = Context.User!.FindFirst("preferred_username")!.Value;
        var groups = _userGroups.GetOrAdd(email, _ => new HashSet<string>());
        lock (groups)
        {
            groups.Add(testRunId);
        }
        var connectionId = _userConnections.GetOrAdd(email, _ => Context.ConnectionId);
        await Groups.AddToGroupAsync(connectionId, testRunId);
        await Clients.Group(testRunId).SendAsync("AddClient", testRunId, $"User: {email} has joined Test Run: {testRunId}");
    }

    public async Task RemoveClient(string testRunId)
    {
        string email = Context.User!.FindFirst("preferred_username")!.Value;
        if (_userGroups.TryGetValue(email, out var groups))
        {
            lock (groups)
            {
                groups.Remove(testRunId);
                if (groups.Count == 0)
                {
                    _userGroups.TryRemove(email, out _);
                }
            }
        }

        if (_userConnections.TryGetValue(email, out var connectionID))
        {
            await Clients.Group(testRunId).SendAsync("RemoveClient", testRunId, $"User: {email} has left Test Run: {testRunId}");
            await Groups.RemoveFromGroupAsync(connectionID, testRunId);
        }
    }

    public async Task GetRuns()
    {
        string email = Context.User!.FindFirst("preferred_username")!.Value;
        if (_userGroups.TryGetValue(email, out var groups))
        {
            await Clients.Caller.SendAsync("GetRuns", groups.ToList(), "Runs fetched successfully");
        }
        else
        {
            await Clients.Caller.SendAsync("GetRuns", new List<string>(), "No runs found");
        }
    }
}
