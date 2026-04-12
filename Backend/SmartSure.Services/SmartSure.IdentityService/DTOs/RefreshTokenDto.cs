using System.ComponentModel.DataAnnotations;

namespace SmartSure.IdentityService.DTOs;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "Refresh token is required.")]
    public string Token { get; set; } = string.Empty;
}