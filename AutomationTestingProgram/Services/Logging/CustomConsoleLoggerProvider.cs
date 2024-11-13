using Microsoft.Extensions.Logging;

public class CustomConsoleLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        // Create a new instance of our custom logger
        return new CustomConsoleLogger(categoryName);
    }

    public void Dispose()
    {
        // No resources to clean up in this case, but we still need to implement Dispose to satisfy the interface.
    }
}
