using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs;

/// <summary>Request body for the admin update-policy-status endpoint.</summary>
public class UpdatePolicyStatusDto
{
    /// <summary>Target status — must be a valid PolicyStatus constant (e.g. "ACTIVE", "CANCELLED", "EXPIRED").</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}