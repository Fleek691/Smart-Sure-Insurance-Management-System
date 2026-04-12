using SmartSure.IdentityService.DTOs;

namespace SmartSure.IdentityService.Services;

public interface IUserAdministrationService
{
    Task<List<ProfileDto>> GetUsersAsync();
    Task UpdateUserStatusAsync(Guid userId, bool isActive);
    Task UpdateUserRoleAsync(Guid userId, string roleName);
}
