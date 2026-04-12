using System.ComponentModel.DataAnnotations;

namespace SmartSure.IdentityService.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 150 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    [StringLength(256, ErrorMessage = "Email must not exceed 256 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string Password { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please provide a valid phone number.")]
    [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters.")]
    public string? PhoneNumber { get; set; }

    [StringLength(300, ErrorMessage = "Address must not exceed 300 characters.")]
    public string? Address { get; set; }
}
