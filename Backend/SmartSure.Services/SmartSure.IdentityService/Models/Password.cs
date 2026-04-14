namespace SmartSure.IdentityService.Models;

/// <summary>
/// Stores the BCrypt-hashed password for a user.
/// Kept in a separate table so the hash is never accidentally included in user queries.
/// </summary>
public class Password
{
    public Guid PassId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>BCrypt hash of the user's plaintext password.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public User? User { get; set; }
}
