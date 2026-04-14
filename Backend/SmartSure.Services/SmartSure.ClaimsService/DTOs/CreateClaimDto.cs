using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs;

/// <summary>Request body for creating a new claim in Draft status.</summary>
public class CreateClaimDto
{
    /// <summary>The active policy to file the claim against.</summary>
    [Required]
    public Guid PolicyId { get; set; }

    /// <summary>Detailed description of the incident or loss (min 10 characters enforced in service).</summary>
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Requested claim amount — must be greater than zero.</summary>
    [Range(1, double.MaxValue)]
    public decimal ClaimAmount { get; set; }
}