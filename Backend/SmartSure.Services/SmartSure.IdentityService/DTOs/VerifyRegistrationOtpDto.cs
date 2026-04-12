using System.ComponentModel.DataAnnotations;

namespace SmartSure.IdentityService.DTOs;

public class VerifyRegistrationOtpDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be a 6-digit number.")]
    public string Otp { get; set; } = string.Empty;
}
