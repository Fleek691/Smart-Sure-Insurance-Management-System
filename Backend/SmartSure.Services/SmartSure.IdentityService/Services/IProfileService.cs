using SmartSure.IdentityService.DTOs;

namespace SmartSure.IdentityService.Services;

public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(Guid userId);
    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
}
