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
    public class CustomService
    {
        public readonly ILogger<CustomService> _logger;

        public CustomService()
        {
            CustomLoggerProvider provider = new CustomLoggerProvider(LogManager.GetRunFolderPath());
            _logger = provider.CreateLogger<CustomService>();
        }
        public async void OnApplicationStopping()
        {
            _logger.LogInformation("Application is stopping -- (Graceful shutdown)");

            string logLevelText = "CRITICAL";
            string text = "SHUTDOWN INITIATED. STOPPING ALL THREADS.";
            string logMessage = string.Format("{0:HH:mm:ss.fff} [{1}] {2}\n", DateTime.Now, logLevelText, text);

            LogManager.FlushAll(logMessage);

            RequestHandler.ShutDownSignal();
            await RequestHandler.ReadyForTermination();
            
            if (_logger is CustomLogger<CustomService> customLogger)
            {
                customLogger.Flush();
            }

            Environment.Exit(0);
        }
    }
}
