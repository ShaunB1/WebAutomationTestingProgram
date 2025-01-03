namespace AutomationTestingProgram.Models.Exceptions
{
    public class PasswordResetLimitException : Exception
    {
        public PasswordResetLimitException(string message)
            : base(message)
        {
        }

        public PasswordResetLimitException(string message, Exception innerException)
            : base(message, innerException) 
        {
        }

        public override string ToString()
        {
            return $"Password Reset Limit Reached: {Message}";
        }
    }
}
