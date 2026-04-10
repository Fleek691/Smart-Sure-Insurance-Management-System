using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs;

public class UpdatePolicyStatusDto
{
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}