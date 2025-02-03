namespace AutomationTestingProgram.Core;

public class CustomLoggerProvider : ICustomLoggerProvider
{
    private readonly string _defaultFilePath;
    
    public CustomLoggerProvider(string logFilePath)
    {
        _defaultFilePath = logFilePath ?? throw new ArgumentNullException($"{nameof(logFilePath)} cannot be null!");
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new CustomLogger(categoryName, _defaultFilePath);
    }

    public ILogger CreateLogger(string categoryName, string logFilePath)
    {
        return new CustomLogger(categoryName, logFilePath);
    }

    public ICustomLogger CreateLogger<T>()
    {
        return new CustomLogger(typeof(T).Name, _defaultFilePath);
    }

    public ICustomLogger CreateLogger<T>(string logFilePath)
    {
        return new CustomLogger(typeof(T).Name, logFilePath);
    }

    public void Dispose()
    {
        
    }
}
