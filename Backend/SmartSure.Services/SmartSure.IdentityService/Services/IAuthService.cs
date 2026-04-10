using SmartSure.IdentityService.DTOs;

namespace SmartSure.IdentityService.Services;

public interface IAuthService
{
    Task<OtpDispatchResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> VerifyRegistrationOtpAsync(VerifyRegistrationOtpDto dto);
    Task<OtpDispatchResponseDto> ResendRegistrationOtpAsync(ResendRegistrationOtpDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task RequestPasswordResetAsync(ForgotPasswordDto dto);
    Task ResetPasswordAsync(ResetPasswordDto dto);
}
