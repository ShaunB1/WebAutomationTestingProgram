using Microsoft.AspNetCore.SignalR;

namespace WebAutomationTestingProgram.Core.Hubs;

public static class HubHelper
{
    public static async Task AddToGroupAsync(IHubContext<TestHub> hubContext, string email, string testRunId)
    {
        var groups = TestHub._userGroups.GetOrAdd(email, _ => new HashSet<string>());
        lock (groups)
        {
            groups.Add(testRunId);
        }

        if (TestHub._userConnections.TryGetValue(email, out var connectionID))
        {
            await hubContext.Groups.AddToGroupAsync(connectionID, testRunId);
            await hubContext.Clients.Group(testRunId).SendAsync("AddClient", $"User: {email} with Connection ID: {connectionID} has joined Test Run: {testRunId}");
        }
    }

    public static async Task RemoveFromGroupAsync(IHubContext<TestHub> hubContext, string email, string testRunId)
    {
        if (TestHub._userGroups.TryGetValue(email, out var groups))
        {
            lock (groups)
            {
                groups.Remove(testRunId);
                if (groups.Count == 0)
                {
                    TestHub._userGroups.TryRemove(email, out _);
                }
            }
        }

        if (TestHub._userConnections.TryGetValue(email, out var connectionID))
        {
            await hubContext.Groups.RemoveFromGroupAsync(connectionID, testRunId);
            await hubContext.Clients.Group(testRunId).SendAsync("RemoveClient", $"User: {email} with Connection ID: {connectionID} has left Test Run: {testRunId}");
        }
    }
}
