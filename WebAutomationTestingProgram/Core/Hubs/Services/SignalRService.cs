using Microsoft.AspNetCore.SignalR;

namespace WebAutomationTestingProgram.Core.Hubs.Services;

public class SignalRService
{
    private readonly IHubContext<TestHub> _hubContext;

    public SignalRService(IHubContext<TestHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastLog(string testRunId, string message)
    {
        if (message.ToLowerInvariant() == "newrun")
        {
            await _hubContext.Clients.All.SendAsync("NewRun", testRunId, message);
        }
        
        await _hubContext.Clients.All.SendAsync("BroadcastLog", testRunId, message);
    }
}