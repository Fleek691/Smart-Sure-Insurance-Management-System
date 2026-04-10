namespace SmartSure.Shared.Exceptions;

public class NotFoundException : SmartSureException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
