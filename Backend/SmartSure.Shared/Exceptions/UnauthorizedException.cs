namespace SmartSure.Shared.Exceptions;

/// <summary>Thrown when authentication is required or credentials are invalid (HTTP 401).</summary>
public class UnauthorizedException : SmartSureException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
