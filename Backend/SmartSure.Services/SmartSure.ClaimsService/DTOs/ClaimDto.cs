namespace SmartSure.ClaimsService.DTOs;

public class ClaimDto
{
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid PolicyId { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal ClaimAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}