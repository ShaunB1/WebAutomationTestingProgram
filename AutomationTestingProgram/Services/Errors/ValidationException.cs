using System;

public class ValidationException : Exception
{
    // Default constructor
    public ValidationException()
    {
    }

    // Constructor that accepts a custom message
    public ValidationException(string message)
        : base(message)
    {
    }

    // Constructor that accepts a custom message and an inner exception
    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    // Optional constructor that accepts error code or custom data (e.g., for more context)
    public ValidationException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    // Custom property to store additional error information (optional)
    public string ErrorCode { get; set; }
}
