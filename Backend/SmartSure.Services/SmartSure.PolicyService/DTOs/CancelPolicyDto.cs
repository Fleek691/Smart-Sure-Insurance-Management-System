using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs;

/// <summary>Request body for the customer cancel-policy endpoint.</summary>
public class CancelPolicyDto
{
    /// <summary>Reason for cancellation — stored on the policy for audit purposes.</summary>
    [Required]
    [MaxLength(256)]
    public string Reason { get; set; } = string.Empty;
}