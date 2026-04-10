namespace SmartSure.Shared.Exceptions;

public class HttpServiceException : SmartSureException
{
    public int StatusCode { get; }

    public HttpServiceException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }
}
