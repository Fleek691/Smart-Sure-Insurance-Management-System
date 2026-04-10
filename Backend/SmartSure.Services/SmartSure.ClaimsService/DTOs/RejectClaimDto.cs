using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs;

public class RejectClaimDto
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}