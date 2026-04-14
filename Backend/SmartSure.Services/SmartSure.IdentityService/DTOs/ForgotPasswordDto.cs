using System.ComponentModel.DataAnnotations;

namespace SmartSure.IdentityService.DTOs;

/// <summary>
/// Request body for the forgot-password endpoint.
/// Triggers a 6-digit OTP email to the provided address (silent on unknown emails).
/// </summary>
public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public string Email { get; set; } = string.Empty;
}
