namespace SmartSure.IdentityService.DTOs;

public class VerifyRegistrationOtpDto
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}
