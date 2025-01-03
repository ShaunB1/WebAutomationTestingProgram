using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Services.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AutomationTestingProgram.Services
{   
    /// <summary>
    /// Service used to ensure GraceFul shutdown of application.
    /// </summary>
    public class ShutDownService
    {
        public readonly ILogger<ShutDownService> _logger;

        public ShutDownService()
        {
            CustomLoggerProvider provider = new CustomLoggerProvider(LogManager.GetRunFolderPath());
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
            
            if (_logger is CustomLogger<ShutDownService> customLogger)
            {
                customLogger.Flush(); // Makes sure console is fully flushed
            }

            Environment.Exit(0); // Closes application
        }
    }
}
