namespace SmartSure.IdentityService.Models;

/// <summary>
/// Join entity that maps a <see cref="User"/> to a <see cref="Role"/>.
/// A user can hold multiple roles simultaneously.
/// </summary>
public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public User? User { get; set; }
    public Role? Role { get; set; }
}
