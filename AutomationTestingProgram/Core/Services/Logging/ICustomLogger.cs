using System.Text;

namespace AutomationTestingProgram.Core;

public interface ICustomLogger : ILogger
{
    public new IDisposable BeginScope<TState>(TState state);

    public new bool IsEnabled(LogLevel logLevel);

    public void Flush(bool removeEntry = true, string message = "");

    public new void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);
}
