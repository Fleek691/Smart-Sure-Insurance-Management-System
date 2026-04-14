namespace SmartSure.IdentityService.Models;

/// <summary>
/// Represents an application role (e.g. Customer, Admin).
/// Roles are seeded at migration time and assigned to users via <see cref="UserRole"/>.
/// </summary>
public class Role
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
