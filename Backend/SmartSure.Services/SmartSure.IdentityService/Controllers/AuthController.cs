using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SmartSure.IdentityService.DTOs;
using SmartSure.IdentityService.Services;

namespace SmartSure.IdentityService.Controllers;

/// <summary>
/// Handles user authentication: registration, login, OAuth, token refresh, and password recovery.
/// </summary>
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

    /// <summary>Starts registration: validates input, caches OTP, and sends verification email.</summary>
    [HttpPost("register")]
    public async Task<OtpDispatchResponseDto> Register([FromBody] RegisterDto dto)
    {
        return await _authService.RegisterAsync(dto);
    }

    /// <summary>Completes registration by verifying the OTP and issuing JWT + refresh token.</summary>
    [HttpPost("register/verify-otp")]
    public async Task<AuthResponseDto> VerifyRegistrationOtp([FromBody] VerifyRegistrationOtpDto dto)
    {
        return await _authService.VerifyRegistrationOtpAsync(dto);
    }

    /// <summary>Re-sends the registration OTP to the same email (resets the expiry window).</summary>
    [HttpPost("register/resend-otp")]
    public async Task<OtpDispatchResponseDto> ResendRegistrationOtp([FromBody] ResendRegistrationOtpDto dto)
    {
        return await _authService.ResendRegistrationOtpAsync(dto);
    }

    /// <summary>Authenticates via email/password and returns JWT + refresh token.</summary>
    [HttpPost("login")]
    public async Task<AuthResponseDto> Login([FromBody] LoginDto dto)
    {
        return await _authService.LoginAsync(dto);
    }

    /// <summary>Returns the Google OAuth consent URL for the frontend to redirect to.</summary>
    [HttpGet("google")]
    public ActionResult<string> GoogleLoginUrl()
    {
        return Ok(_googleAuthService.GetGoogleLoginUrl());
    }

    /// <summary>Handles Google OAuth callback: exchanges auth code for tokens and redirects to frontend.</summary>
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

    /// <summary>Issues a new JWT using a valid refresh token (rotation — old token is invalidated).</summary>
    [HttpPost("refresh-token")]
    public async Task<AuthResponseDto> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        return await _authService.RefreshTokenAsync(dto);
    }

    /// <summary>Sends a password-reset OTP to the user's email (silent on unknown emails).</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.RequestPasswordResetAsync(dto);
        return Ok();
    }

    /// <summary>Resets the password using the OTP received via email.</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok();
    }
}
