using SmartSure.IdentityService.Models;

namespace SmartSure.IdentityService.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName);
    Task<List<Role>> GetAllAsync();
}
