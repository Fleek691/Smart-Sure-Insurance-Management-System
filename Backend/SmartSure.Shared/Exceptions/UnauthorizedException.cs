namespace SmartSure.Shared.Exceptions;

public class UnauthorizedException : SmartSureException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
