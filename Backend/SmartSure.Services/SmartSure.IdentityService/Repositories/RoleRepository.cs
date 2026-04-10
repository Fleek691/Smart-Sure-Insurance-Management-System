using Microsoft.EntityFrameworkCore;
using SmartSure.IdentityService.Data;
using SmartSure.IdentityService.Models;

namespace SmartSure.IdentityService.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _context;

    public RoleRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public Task<Role?> GetByNameAsync(string roleName)
    {
        return GetByNameInternalAsync(roleName);
    }

    public Task<List<Role>> GetAllAsync()
    {
        return _context.Roles.ToListAsync();
    }

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
