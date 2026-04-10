namespace SmartSure.Shared.Exceptions;

public abstract class SmartSureException : Exception
{
    protected SmartSureException(string message) : base(message)
    {
    }
}
