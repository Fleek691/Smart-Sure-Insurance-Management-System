namespace SmartSure.IdentityService.DTOs;

/// <summary>
/// Returned after a successful login, OTP verification, or token refresh.
/// Contains the JWT access token, a refresh token, and basic user info.
/// </summary>
public class AuthResponseDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Short-lived JWT access token (60-minute expiry).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Long-lived opaque token used to obtain a new JWT (7-day expiry).</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the access token expires.</summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>List of role names assigned to the user (e.g. "Customer", "Admin").</summary>
    public List<string> Roles { get; set; } = new();
}
