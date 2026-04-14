namespace SmartSure.Shared.Exceptions;

/// <summary>Thrown when a requested resource is not found (HTTP 404).</summary>
public class NotFoundException : SmartSureException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
