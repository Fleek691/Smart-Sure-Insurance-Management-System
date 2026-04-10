using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SmartSure.IdentityService.DTOs;
using SmartSure.IdentityService.Services;

namespace SmartSure.IdentityService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IGoogleAuthService googleAuthService,IConfiguration configuration)
    {
        _authService = authService;
        _googleAuthService = googleAuthService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<OtpDispatchResponseDto> Register([FromBody] RegisterDto dto)
    {
        return await _authService.RegisterAsync(dto);
    }

    [HttpPost("register/verify-otp")]
    public async Task<AuthResponseDto> VerifyRegistrationOtp([FromBody] VerifyRegistrationOtpDto dto)
    {
        return await _authService.VerifyRegistrationOtpAsync(dto);
    }

    [HttpPost("register/resend-otp")]
    public async Task<OtpDispatchResponseDto> ResendRegistrationOtp([FromBody] ResendRegistrationOtpDto dto)
    {
        return await _authService.ResendRegistrationOtpAsync(dto);
    }

    [HttpPost("login")]
    public async Task<AuthResponseDto> Login([FromBody] LoginDto dto)
    {
        return await _authService.LoginAsync(dto);
    }

    [HttpGet("google")]
    public ActionResult<string> GoogleLoginUrl()
    {
        return Ok(_googleAuthService.GetGoogleLoginUrl());
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var code = Request.Query["code"].ToString();
        var dto = new GoogleCallbackDto { Code = code };
        var session = await _googleAuthService.ProcessGoogleCallbackAsync(dto);

        var frontendCallbackUrl = _configuration["Google:FrontendCallbackUrl"]
            ?? "http://localhost:4200/auth/google/callback";

        var redirectUrl = QueryHelpers.AddQueryString(frontendCallbackUrl, new Dictionary<string, string?>
        {
            ["userId"]       = session.UserId.ToString(),
            ["fullName"]     = session.FullName,
            ["email"]        = session.Email,
            ["token"]        = session.Token,
            ["refreshToken"] = session.RefreshToken,
            ["expiresAtUtc"] = session.ExpiresAtUtc.ToString("O"),
            ["roles"]        = string.Join(",", session.Roles)
        });

        return Redirect(redirectUrl);
    }

    [HttpPost("refresh-token")]
    public async Task<AuthResponseDto> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        return await _authService.RefreshTokenAsync(dto);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.RequestPasswordResetAsync(dto);
        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok();
    }
}
