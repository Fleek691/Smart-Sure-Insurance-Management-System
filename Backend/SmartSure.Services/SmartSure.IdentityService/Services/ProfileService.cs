using Microsoft.Extensions.Caching.Memory;
using SmartSure.IdentityService.DTOs;
using SmartSure.IdentityService.Models;
using SmartSure.IdentityService.Repositories;
using SmartSure.Shared.Exceptions;

namespace SmartSure.IdentityService.Services;

public class ProfileService(IMemoryCache memoryCache, IUserRepository userRepository) : IProfileService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<ProfileDto> GetProfileAsync(Guid userId)
    {
        if (_memoryCache.TryGetValue(GetUserProfileCacheKey(userId), out ProfileDto? cachedProfile) && cachedProfile is not null)
        {
            return cachedProfile;
        }

        var user = await _userRepository.GetByIdAsync(userId)
                   ?? throw new NotFoundException("User not found.");
        var profile = MapProfile(user);
        CacheUserProfile(user.UserId, profile);
        return profile;
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId)
                   ?? throw new NotFoundException("User not found.");

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;
        await _userRepository.SaveChangesAsync();

        InvalidateUserCache(user.UserId);
        var profile = MapProfile(user);
        CacheUserProfile(user.UserId, profile);
        return profile;
    }

    private static ProfileDto MapProfile(User user)
    {
        var roles = new List<string>();

        foreach (var userRole in user.UserRoles)
        {
            var roleName = userRole.Role?.RoleName;
            if (!string.IsNullOrWhiteSpace(roleName) && !roles.Contains(roleName))
            {
                roles.Add(roleName);
            }
        }

        return new ProfileDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            IsActive = user.IsActive,
            Roles = roles
        };
    }

    private void CacheUserProfile(Guid userId, ProfileDto profile)
    {
        _memoryCache.Set(GetUserProfileCacheKey(userId), profile, TimeSpan.FromMinutes(5));
    }

    private void InvalidateUserCache(Guid userId)
    {
        _memoryCache.Remove(GetUserRoleCacheKey(userId));
        _memoryCache.Remove(GetUserProfileCacheKey(userId));
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
