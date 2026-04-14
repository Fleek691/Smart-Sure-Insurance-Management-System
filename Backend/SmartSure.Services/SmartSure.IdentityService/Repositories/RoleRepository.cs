using Microsoft.EntityFrameworkCore;
using SmartSure.IdentityService.Data;
using SmartSure.IdentityService.Models;

namespace SmartSure.IdentityService.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRoleRepository"/>.
/// Role lookups are case-insensitive to handle mixed-case role names gracefully.
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _context;

    public RoleRepository(IdentityDbContext context)
    {
        _context = context;
    }

    /// <summary>Finds a role by name (case-insensitive). Returns null if not found.</summary>
    public Task<Role?> GetByNameAsync(string roleName)
    {
        return GetByNameInternalAsync(roleName);
    }

    /// <summary>Returns all roles in the system.</summary>
    public Task<List<Role>> GetAllAsync()
    {
        return _context.Roles.ToListAsync();
    }

    /// <summary>
    /// Loads all roles into memory and performs a case-insensitive name match.
    /// This avoids collation issues when the database uses a case-sensitive collation.
    /// </summary>
    private async Task<Role?> GetByNameInternalAsync(string roleName)
    {
        var roles = await _context.Roles.ToListAsync();

        foreach (var role in roles)
        {
            if (string.Equals(role.RoleName, roleName, StringComparison.OrdinalIgnoreCase))
            {
                return role;
            }
        }

        return null;
    }
}
