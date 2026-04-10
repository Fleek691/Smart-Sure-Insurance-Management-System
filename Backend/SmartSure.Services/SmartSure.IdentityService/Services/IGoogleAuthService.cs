using SmartSure.IdentityService.DTOs;

namespace SmartSure.IdentityService.Services;

public interface IGoogleAuthService
{
    string GetGoogleLoginUrl();
    Task<AuthResponseDto> ProcessGoogleCallbackAsync(GoogleCallbackDto dto);
}