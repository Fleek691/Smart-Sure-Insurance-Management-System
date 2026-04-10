using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs;

public class ApproveClaimDto
{
    [Range(0, double.MaxValue)]
    public decimal? ApprovedAmount { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}