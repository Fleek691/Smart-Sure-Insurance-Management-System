namespace SmartSure.IdentityService.Helpers;

/// <summary>
/// Provides JWT access token creation and refresh token generation.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Builds a signed JWT containing identity and role claims for the given user.
    /// </summary>
    string BuildToken(string key, string issuer, IEnumerable<string> audiences, string userId, string email, IEnumerable<string> roles);

    /// <summary>
    /// Generates a cryptographically random, opaque refresh token (Base64-encoded).
    /// </summary>
    string GenerateRefreshToken();
}