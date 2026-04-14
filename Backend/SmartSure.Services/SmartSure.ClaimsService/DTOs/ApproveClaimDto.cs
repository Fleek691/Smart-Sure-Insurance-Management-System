using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs;

/// <summary>
/// Request body for the admin approve-claim endpoint.
/// <see cref="ApprovedAmount"/> defaults to the original claim amount if omitted.
/// </summary>
public class ApproveClaimDto
{
    /// <summary>Override the payout amount — leave null to approve the full claimed amount.</summary>
    [Range(0, double.MaxValue)]
    public decimal? ApprovedAmount { get; set; }

    /// <summary>Optional internal note recorded on the claim.</summary>
    [MaxLength(500)]
    public string? Note { get; set; }
}