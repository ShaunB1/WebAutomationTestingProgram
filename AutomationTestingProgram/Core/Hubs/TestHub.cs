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
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userGroups = new ConcurrentDictionary<string, HashSet<string>>();
    // Email -> Connection ID
    private static readonly ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();

    // Will create a new test run group
    public async Task AddGroup(string email, string testRunId)
    {

        var groups = _userGroups.GetOrAdd(email, _ => new HashSet<string>());
        lock (groups)
        {
            groups.Add(testRunId);
        }

        if (_userConnections.TryGetValue(email, out var connectionID))
        {
            await Groups.AddToGroupAsync(connectionID, testRunId);
            string username = Context!.User!.Identity!.Name!;
            await Clients.Group(testRunId).SendAsync("AddClient", $"User: {username} with Connection ID: {Context.ConnectionId} has joined Test Run: {testRunId}");
        }
    }

    // Will remove a test run group
    public async Task RemoveGroup(string email, string testRunId)
    {
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
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, testRunId);
            string username = Context!.User!.Identity!.Name!;
            await Clients.Group(testRunId).SendAsync("RemoveClient", $"User: {username} with Connection ID: {Context.ConnectionId} has left Test Run: {testRunId}");
        }
    }

    // When user connects (or re-connects)
    public override async Task OnConnectedAsync()
    {
        string username = Context.User!.Identity!.Name!;
        string email = Context.User!.FindFirst(ClaimTypes.Email)!.Value;

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
        string email = Context.User!.FindFirst(ClaimTypes.Email)!.Value;

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

/* Issues:
 * - Users cannot see other users logs
 *
 * 
 * 
 */
