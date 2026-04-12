using Microsoft.Extensions.Caching.Memory;
using SmartSure.IdentityService.DTOs;
using SmartSure.IdentityService.Repositories;
using SmartSure.Shared.Exceptions;

namespace SmartSure.IdentityService.Services;

public class UserAdministrationService(IMemoryCache memoryCache, IUserRepository userRepository, IRoleRepository roleRepository) : IUserAdministrationService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRoleRepository _roleRepository = roleRepository;

    public async Task<List<ProfileDto>> GetUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var profiles = new List<ProfileDto>();

        foreach (var user in users)
        {
            profiles.Add(new ProfileDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                IsActive = user.IsActive,
                Roles = user.UserRoles
                    .Select(x => x.Role?.RoleName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .Distinct(StringComparer.Ordinal)
                    .ToList()
            });
        }

        return profiles;
    }

    public async Task UpdateUserStatusAsync(Guid userId, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(userId)
                   ?? throw new NotFoundException("User not found.");

        user.IsActive = isActive;
        await _userRepository.SaveChangesAsync();

        _memoryCache.Remove(GetUserRoleCacheKey(user.UserId));
        _memoryCache.Remove(GetUserProfileCacheKey(user.UserId));
    }

    public async Task UpdateUserRoleAsync(Guid userId, string roleName)
    {
        var normalizedRole = roleName?.Trim().ToUpperInvariant();
        if (normalizedRole != "CUSTOMER" && normalizedRole != "ADMIN")
        {
            throw new ValidationException("Role must be either CUSTOMER or ADMIN.");
        }

        var user = await _userRepository.GetByIdAsync(userId)
                   ?? throw new NotFoundException("User not found.");

        var role = await _roleRepository.GetByNameAsync(normalizedRole);
        if (role is null)
        {
            role = new Models.Role { RoleId = Guid.NewGuid(), RoleName = normalizedRole };
        }

        user.UserRoles.Clear();
        user.UserRoles.Add(new Models.UserRole { RoleId = role.RoleId, Role = role });
        await _userRepository.SaveChangesAsync();

        _memoryCache.Remove(GetUserRoleCacheKey(user.UserId));
        _memoryCache.Remove(GetUserProfileCacheKey(user.UserId));
    }

    private static string GetUserRoleCacheKey(Guid userId)
    {
        return $"user_role_{userId}";
    }

    private static string GetUserProfileCacheKey(Guid userId)
    {
        return $"user_profile_{userId}";
    }
}
