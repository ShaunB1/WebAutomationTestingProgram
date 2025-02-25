
using AutomationTestingProgram.Core.Services.Logging;

namespace AutomationTestingProgram.Core.Services.ApplicationLifetime;

/// <summary>
/// Service used to ensure GraceFul shutdown of application.
/// </summary>
public class ShutDownService
{
    private readonly ICustomLogger _logger;
    private readonly RequestHandler _handler;

    public ShutDownService(ICustomLoggerProvider provider, RequestHandler handler)
    {
        _logger = provider.CreateLogger<ShutDownService>();
        _handler = handler;
    }

    public async Task OnApplicationStopping()
    {
        _logger.LogInformation("Application is stopping -- (Graceful shutdown)");

        string logLevelText = "CRITICAL";
        string text = "SHUTDOWN INITIATED. STOPPING ALL THREADS.";
        string logMessage = string.Format("[{0:HH:mm:ss.fff}] [{1}] {2}\n", DateTime.Now, logLevelText, text);

        LogManager.FlushAll(logMessage); // Sends shutdown message to all active logs.

        await _handler.ShutDownAsync(); // Waits until all requests are terminated

        _logger.Flush();

        Environment.Exit(0); // Closes application
    }
}
