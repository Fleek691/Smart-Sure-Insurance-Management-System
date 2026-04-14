namespace SmartSure.IdentityService.Models;

/// <summary>
/// A single-use, time-limited OTP token for password reset.
/// Multiple tokens can exist per user; only the most recent unused one is valid.
/// </summary>
public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    /// <summary>6-digit numeric OTP sent to the user's email.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>UTC timestamp after which this token is no longer accepted.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>True once the token has been consumed to prevent replay attacks.</summary>
    public bool IsUsed { get; set; }

    public User? User { get; set; }
}
