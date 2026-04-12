namespace SmartSure.Shared.Exceptions;

/// <summary>
/// Thrown when an authenticated user attempts an action they are not permitted to perform (HTTP 403).
/// Use <see cref="UnauthorizedException"/> for unauthenticated access (HTTP 401).
/// </summary>
public class ForbiddenException : SmartSureException
{
    public ForbiddenException(string message) : base(message) { }
}
