namespace SmartSure.Shared.Exceptions;

/// <summary>Thrown when input validation fails (HTTP 400).</summary>
public class ValidationException : SmartSureException
{
    public ValidationException(string message) : base(message)
    {
    }
}
