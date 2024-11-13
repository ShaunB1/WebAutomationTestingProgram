using Microsoft.Extensions.Logging;
using System;
using System.Text;

public class CustomConsoleLogger : ILogger
{
    private readonly string _categoryName;

    public CustomConsoleLogger(string categoryName)
    {
        _categoryName = categoryName;
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
        // Custom log formatting
        var logMessage = new StringBuilder();
        logMessage.AppendFormat("{0:HH:mm:ss.fff} ", DateTime.Now);  // Timestamp
                                                                
        string logLevelText = logLevel.ToString().ToUpper(); // LogLevel
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

        logMessage.AppendFormat("[{0}] ", coloredLogLevelText);  // Log level
        logMessage.AppendLine(formatter(state, exception));  // Log message

        if (exception != null)
        {
            logMessage.AppendLine(exception.ToString());  // Exception stack trace
        }

        // Output log to the console
        Console.Write(logMessage.ToString());
    }
}

// This is a simple implementation of IDisposable that does nothing, just to satisfy the interface.
public class NullDisposable : IDisposable
{
    public static readonly NullDisposable Instance = new NullDisposable();

    public void Dispose() { }
}
