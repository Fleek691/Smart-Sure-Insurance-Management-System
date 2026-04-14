using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs;

/// <summary>Request body for the admin mark-under-review endpoint. Note is optional.</summary>
public class ReviewClaimDto
{
    [MaxLength(500)]
    public string? Note { get; set; }
}