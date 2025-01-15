
namespace AutomationTestingProgram.Core;

/// <summary>
/// Service used to ensure GraceFul shutdown of application.
/// </summary>
public class ShutDownService
{
    private readonly ICustomLogger _logger;

    public ShutDownService(ICustomLoggerProvider provider)
    {
        _logger = provider.CreateLogger<ShutDownService>();
    }

    public async Task OnApplicationStopping()
    {
        _logger.LogInformation("Application is stopping -- (Graceful shutdown)");

        string logLevelText = "CRITICAL";
        string text = "SHUTDOWN INITIATED. STOPPING ALL THREADS.";
        string logMessage = string.Format("{0:HH:mm:ss.fff} [{1}] {2}\n", DateTime.Now, logLevelText, text);

        LogManager.FlushAll(logMessage); // Sends shutdown message to all active logs.

        await RequestHandler.ShutDownAsync(); // Waits until all requests are terminated

        _logger.Flush();

        Environment.Exit(0); // Closes application
    }
}
