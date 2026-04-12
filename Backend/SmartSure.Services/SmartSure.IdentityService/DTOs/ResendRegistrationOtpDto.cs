using System.ComponentModel.DataAnnotations;

namespace SmartSure.IdentityService.DTOs;

public class ResendRegistrationOtpDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public string Email { get; set; } = string.Empty;
}
