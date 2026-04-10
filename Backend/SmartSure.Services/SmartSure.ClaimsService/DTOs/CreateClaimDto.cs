using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs;

public class CreateClaimDto
{
    [Required]
    public Guid PolicyId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, double.MaxValue)]
    public decimal ClaimAmount { get; set; }
}