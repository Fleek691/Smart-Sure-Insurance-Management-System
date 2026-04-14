namespace SmartSure.Shared.Exceptions;

/// <summary>
/// Thrown when a downstream HTTP service call fails. Carries the upstream status code.
/// </summary>
public class HttpServiceException : SmartSureException
{
    public int StatusCode { get; }

    public HttpServiceException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }
}
