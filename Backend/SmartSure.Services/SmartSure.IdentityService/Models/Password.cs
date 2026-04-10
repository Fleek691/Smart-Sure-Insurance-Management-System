namespace SmartSure.IdentityService.Models;

public class Password
{
    public Guid PassId { get; set; }
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;

    public User? User { get; set; }
}
