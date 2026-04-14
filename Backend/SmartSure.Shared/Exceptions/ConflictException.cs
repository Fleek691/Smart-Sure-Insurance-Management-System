namespace SmartSure.Shared.Exceptions;

/// <summary>Thrown when a resource conflict occurs (HTTP 409).</summary>
public class ConflictException : SmartSureException
{
    public ConflictException(string message) : base(message)
    {
    }
}
