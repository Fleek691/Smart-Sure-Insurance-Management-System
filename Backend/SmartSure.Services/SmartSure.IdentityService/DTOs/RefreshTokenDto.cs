using System.ComponentModel.DataAnnotations;

namespace SmartSure.IdentityService.DTOs;

/// <summary>Request body for the token refresh endpoint.</summary>
public class RefreshTokenDto
{
    [Required(ErrorMessage = "Refresh token is required.")]
    public string Token { get; set; } = string.Empty;
}