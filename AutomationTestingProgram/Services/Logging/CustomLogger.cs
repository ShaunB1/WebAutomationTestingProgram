using AutomationTestingProgram.Services.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

public class CustomLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logFilePath;

    public CustomLogger(string categoryName, string logFilePath)
    {
        _categoryName = categoryName;
        _logFilePath = logFilePath;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return NullDisposable.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var logMessage = new StringBuilder();
        logMessage.AppendFormat("{0:HH:mm:ss.fff} ", DateTime.Now);
                                                                
        string logLevelText = logLevel.ToString().ToUpper();
        string coloredLogLevelText = logLevelText;

        switch (logLevel)
        {
            case LogLevel.Information:
                logLevelText = "INFO";
                coloredLogLevelText = $"\x1b[32m{logLevelText}\x1b[0m";  // Green for Information
                break;
            case LogLevel.Warning:
                logLevelText = "WARN";
                coloredLogLevelText = $"\x1b[33m{logLevelText}\x1b[0m";  // Yellow for Warning
                break;
            case LogLevel.Error:
                logLevelText = "ERROR";
                coloredLogLevelText = $"\x1b[31m{logLevelText}\x1b[0m";  // Red for Error and Critical
                break;
            case LogLevel.Critical:
                logLevelText = "CRITICAL";
                coloredLogLevelText = $"\x1b[31m{logLevelText}\x1b[0m";  // Red for Error and Critical
                break;
            default:
                coloredLogLevelText = $"\x1b[37m{logLevelText}\x1b[0m";  // Default color for other levels (e.g., Debug, Trace)
                break;
        }

        logMessage.AppendFormat("[{0}] ", coloredLogLevelText);
        logMessage.AppendLine(formatter(state, exception));

        if (exception != null)
        {
            logMessage.AppendLine(exception.ToString());
        }


        if (_logFilePath.Equals("Console") || _logFilePath.Equals(LogManager.GetRunFolderPath()))
        {
            Console.WriteLine(logMessage.ToString());
            LogManager.LogAsync(LogManager.GetRunFolderPath(), logMessage.ToString()).Wait();
        }
        else
        {
            LogManager.LogAsync(_logFilePath, logMessage.ToString()).Wait();
        }

    }
}

// This is a simple implementation of IDisposable that does nothing, just to satisfy the interface.
public class NullDisposable : IDisposable
{
    public static readonly NullDisposable Instance = new NullDisposable();

    public void Dispose() { }
}
