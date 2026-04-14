using System.ComponentModel.DataAnnotations;

namespace SmartSure.IdentityService.DTOs;

/// <summary>Request body for the email/password login endpoint.</summary>
public class LoginDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
