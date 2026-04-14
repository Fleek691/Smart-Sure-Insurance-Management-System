using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs;

/// <summary>Request body for the admin reject-claim endpoint. A reason is mandatory.</summary>
public class RejectClaimDto
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}