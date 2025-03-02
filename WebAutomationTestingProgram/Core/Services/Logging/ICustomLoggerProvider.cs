namespace WebAutomationTestingProgram.Core.Services.Logging
{
    public interface ICustomLoggerProvider : ILoggerProvider
    {
        public new ILogger CreateLogger(string categoryName);

        public ILogger CreateLogger(string categoryName, string logFilePath);

        public ICustomLogger CreateLogger<T>();

        public ICustomLogger CreateLogger<T>(string logFilePath);

        public new void Dispose();
    }
}
