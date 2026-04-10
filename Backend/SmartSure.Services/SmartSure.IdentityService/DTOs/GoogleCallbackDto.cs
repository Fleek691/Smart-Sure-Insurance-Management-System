using System.ComponentModel.DataAnnotations;

namespace SmartSure.IdentityService.DTOs;

public class GoogleCallbackDto
{
    [Required]
    public string Code { get; set; } = string.Empty;
}

public class GoogleUserInfoDto
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string Picture { get; set; } = string.Empty;
}