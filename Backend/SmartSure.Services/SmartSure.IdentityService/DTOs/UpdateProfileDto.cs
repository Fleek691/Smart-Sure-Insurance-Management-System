using System.ComponentModel.DataAnnotations;

namespace SmartSure.IdentityService.DTOs;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 150 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please provide a valid phone number.")]
    [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters.")]
    public string? PhoneNumber { get; set; }

    [StringLength(300, ErrorMessage = "Address must not exceed 300 characters.")]
    public string? Address { get; set; }
}
