namespace SmartSure.IdentityService.Models;

/// <summary>
/// Represents a registered user in the system.
/// Supports both email/password and Google OAuth sign-in.
/// </summary>
public class User
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    /// <summary>False when an admin has deactivated the account.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Google OAuth subject claim — null for email/password accounts.</summary>
    public string? GoogleSubject { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Opaque refresh token used to issue new JWTs without re-authentication.</summary>
    public string? RefreshToken { get; set; }

    /// <summary>UTC expiry of the current refresh token (7-day sliding window).</summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // ── Navigation properties ──────────────────────────────────────────────
    public Password? Password { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}