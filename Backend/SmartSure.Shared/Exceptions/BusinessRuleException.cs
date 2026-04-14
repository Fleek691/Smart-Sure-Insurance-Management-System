namespace SmartSure.Shared.Exceptions;

/// <summary>Thrown when a business rule is violated (HTTP 422).</summary>
public class BusinessRuleException : SmartSureException
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
