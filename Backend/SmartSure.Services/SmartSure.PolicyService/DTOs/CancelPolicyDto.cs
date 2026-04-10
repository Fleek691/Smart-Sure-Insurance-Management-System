using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs;

public class CancelPolicyDto
{
    [Required]
    [MaxLength(256)]
    public string Reason { get; set; } = string.Empty;
}