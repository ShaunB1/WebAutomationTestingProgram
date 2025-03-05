namespace WebAutomationTestingProgram.Modules.TestRunnerV2.Models.Exceptions
{
    public class LaunchException : Exception
    {
        public LaunchException(string message)
            : base(message)
        {
        }

        public LaunchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
