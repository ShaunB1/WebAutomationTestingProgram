namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public class PasswordAlreadyResetException : Exception
    {
        public PasswordAlreadyResetException(string message)
            : base(message)
        {
        }

        public PasswordAlreadyResetException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public override string ToString()
        {
            return $"Password Reset Limit Reached: {Message}";
        }
    }
}
