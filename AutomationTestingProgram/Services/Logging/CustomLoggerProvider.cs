using Microsoft.Extensions.Logging;

public class CustomLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;
    
    public CustomLoggerProvider(string logFilePath)
    {
        _logFilePath = logFilePath ?? throw new ArgumentNullException($"{nameof(logFilePath)} cannot be null!");
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new CustomLogger<object>(categoryName, _logFilePath);
    }

    public ILogger<T> CreateLogger<T>()
    {
        return new CustomLogger<T>(typeof(T).Name, _logFilePath);
    }

    public void Dispose()
    {
        
    }
}
