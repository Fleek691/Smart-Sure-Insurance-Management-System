namespace SmartSure.Shared.Exceptions;

/// <summary>
/// Base exception for all SmartSure domain-specific errors.
/// Subclasses map to specific HTTP status codes via the global exception handler.
/// </summary>
public abstract class SmartSureException : Exception
{
    protected SmartSureException(string message) : base(message)
    {
    }
}
