using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs;

public class ReviewClaimDto
{
    [MaxLength(500)]
    public string? Note { get; set; }
}