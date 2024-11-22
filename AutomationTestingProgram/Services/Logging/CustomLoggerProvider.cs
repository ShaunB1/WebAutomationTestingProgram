using Microsoft.Extensions.Logging;

public class CustomLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;
    
    public CustomLoggerProvider(string logFilePath)
    {
        _logFilePath = logFilePath ?? throw new ArgumentNullException($"{nameof(logFilePath)} cannot be null!");
    }
    
    public ILogger CreateLogger(string categoryName) // For non generic loggers
    {
        return new CustomLogger(categoryName, _logFilePath);
    }

    public ILogger<T>? CreateLogger<T>()
    {
        return new CustomLogger(typeof(T).Name, _logFilePath) as ILogger<T>;
    }

    public void Dispose()
    {
        
    }
}
